#!/bin/bash
# Setup Self Contained Script
# Location: scripts/setup/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


# RefactorMCP Self-Contained Setup Script
# Creates a production deployment with Seq logging and systemd service

set -e

# Configuration
REFACTOR_MCP_DIR="/opt/refactor-mcp"
SEQ_DATA_DIR="/opt/seq/data"
CURRENT_DIR=$(pwd)
USER_NAME=$(whoami)

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_status() { echo -e "${BLUE}[INFO]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Clean up Podman environment traces
cleanup_podman() {
    print_status "Cleaning up Podman environment traces..."
    if [[ "$DOCKER_HOST" == *"podman"* ]]; then
        print_warning "Found DOCKER_HOST pointing to Podman socket, unsetting it"
        unset DOCKER_HOST
        export DOCKER_HOST=""
    fi
    if docker ps >/dev/null 2>&1; then
        print_success "Docker is working correctly"
    else
        print_error "Docker connection failed"
        exit 1
    fi
}

# Setup Seq container
setup_seq() {
    print_status "Setting up Seq container with persistent storage..."
    
    sudo mkdir -p $SEQ_DATA_DIR
    sudo chown 5341:5341 $SEQ_DATA_DIR
    
    docker stop refactor-mcp-seq 2>/dev/null || true
    docker rm refactor-mcp-seq 2>/dev/null || true
    
    docker run -d \
        --name refactor-mcp-seq \
        --restart unless-stopped \
        -p 5341:5341 \
        -v $SEQ_DATA_DIR:/data \
        -e ACCEPT_EULA=Y \
        datalust/seq:latest
    
    print_success "Seq container created and started"
    print_status "Seq will be available at: http://localhost:5341"
}

# Install self-contained application
install_app() {
    print_status "Installing self-contained RefactorMCP application..."
    
    # Use the working self-contained build
    if [[ ! -f "./publish-console-safer/RefactorMCP.ConsoleApp" ]]; then
        print_error "Self-contained executable not found. Please run the build first."
        exit 1
    fi
    
    sudo mkdir -p $REFACTOR_MCP_DIR
    sudo cp -r ./publish-console-safer/* $REFACTOR_MCP_DIR/
    sudo chown -R $USER:$USER $REFACTOR_MCP_DIR
    sudo chmod +x $REFACTOR_MCP_DIR/RefactorMCP.ConsoleApp
    
    # Create logs directory
    sudo mkdir -p /var/log/refactor-mcp
    sudo chown $USER:$USER /var/log/refactor-mcp
    
    print_success "Self-contained application installed to $REFACTOR_MCP_DIR"
}

# Create systemd service
create_systemd_service() {
    print_status "Creating systemd service for RefactorMCP..."
    
    sudo tee /etc/systemd/system/refactor-mcp.service > /dev/null <<EOF
[Unit]
Description=RefactorMCP MCP Server (Self-Contained)
Documentation=https://github.com/your-repo/refactor-mcp
After=network.target docker.service
Wants=docker.service

[Service]
Type=simple
ExecStart=$REFACTOR_MCP_DIR/RefactorMCP.ConsoleApp
WorkingDirectory=$REFACTOR_MCP_DIR
User=$USER
Group=$USER
Environment=ASPNETCORE_ENVIRONMENT=Production
Restart=always
RestartSec=10
SyslogIdentifier=refactor-mcp

# Security settings
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=$REFACTOR_MCP_DIR
ReadWritePaths=/tmp
ReadWritePaths=/var/tmp
ReadWritePaths=/var/log/refactor-mcp

[Install]
WantedBy=multi-user.target
EOF

    sudo systemctl daemon-reload
    sudo systemctl enable refactor-mcp.service
    
    print_success "Systemd service created and enabled"
}

# Create management script
create_management_script() {
    print_status "Creating management script..."
    
    sudo tee /usr/local/bin/refactor-mcp-ctl > /dev/null <<'EOF'
#!/bin/bash

# Clean up Podman environment traces
if [[ "$DOCKER_HOST" == *"podman"* ]]; then
    unset DOCKER_HOST
    export DOCKER_HOST=""
fi

case "$1" in
    start)
        echo "Starting RefactorMCP services..."
        docker start refactor-mcp-seq
        sleep 5
        sudo systemctl start refactor-mcp
        ;;
    stop)
        echo "Stopping RefactorMCP services..."
        sudo systemctl stop refactor-mcp
        docker stop refactor-mcp-seq
        ;;
    restart)
        echo "Restarting RefactorMCP services..."
        sudo systemctl stop refactor-mcp
        docker restart refactor-mcp-seq
        sleep 10
        sudo systemctl start refactor-mcp
        ;;
    status)
        echo "=== RefactorMCP Service Status ==="
        sudo systemctl status refactor-mcp --no-pager -l
        echo ""
        echo "=== Seq Container Status ==="
        docker ps -f name=refactor-mcp-seq
        ;;
    logs)
        echo "=== RefactorMCP Logs ==="
        sudo journalctl -u refactor-mcp -f --no-pager
        ;;
    seq-logs)
        echo "=== Seq Container Logs ==="
        docker logs -f refactor-mcp-seq
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|status|logs|seq-logs}"
        exit 1
        ;;
esac
EOF

    sudo chmod +x /usr/local/bin/refactor-mcp-ctl
    
    print_success "Management script created at /usr/local/bin/refactor-mcp-ctl"
}

# Main execution
main() {
    print_status "Starting RefactorMCP self-contained setup..."
    
    if [[ $EUID -eq 0 ]]; then
        print_error "Don't run this script as root. It will ask for sudo when needed."
        exit 1
    fi
    
    if ! sudo -n true 2>/dev/null; then
        print_warning "This script requires sudo access. You'll be prompted for your password."
        sudo -v
    fi
    
    cleanup_podman
    setup_seq
    install_app
    create_systemd_service
    create_management_script
    
    print_success "Setup completed!"
    echo ""
    echo "🎉 RefactorMCP Self-Contained Services are now installed!"
    echo ""
    echo "📋 Available commands:"
    echo "   refactor-mcp-ctl start     - Start all services"
    echo "   refactor-mcp-ctl stop      - Stop all services"  
    echo "   refactor-mcp-ctl restart   - Restart all services"
    echo "   refactor-mcp-ctl status    - Show service status"
    echo "   refactor-mcp-ctl logs      - Show RefactorMCP logs"
    echo "   refactor-mcp-ctl seq-logs  - Show Seq container logs"
    echo ""
    echo "🌐 Services will be available at:"
    echo "   RefactorMCP MCP Server: stdio (for MCP clients)"
    echo "   Seq Logging: http://localhost:5341"
    echo ""
    echo "🚀 Starting services now..."
    refactor-mcp-ctl start
    
    print_success "All services started successfully!"
    print_status "RefactorMCP is running as a self-contained application (no .NET runtime required)!"
}

# Run main function
main "$@"