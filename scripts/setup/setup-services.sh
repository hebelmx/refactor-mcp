#!/bin/bash
# Setup Services Script
# Location: scripts/setup/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


# RefactorMCP Services Setup Script
# This script sets up Seq container and RefactorMCP as system services

set -e

echo "🚀 Setting up RefactorMCP Services..."

# Configuration
SEQ_DATA_DIR="/opt/seq/data"
REFACTOR_MCP_DIR="/opt/refactor-mcp"
CURRENT_DIR=$(pwd)
USER_NAME=$(whoami)

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if running as root
check_sudo() {
    if [[ $EUID -eq 0 ]]; then
        print_error "Don't run this script as root. It will ask for sudo when needed."
        exit 1
    fi
    
    # Test sudo access
    if ! sudo -n true 2>/dev/null; then
        print_warning "This script requires sudo access. You'll be prompted for your password."
        sudo -v
    fi
}

# Install Docker if not present
install_docker() {
    if ! command -v docker &> /dev/null; then
        print_status "Installing Docker..."
        curl -fsSL https://get.docker.com -o get-docker.sh
        sudo sh get-docker.sh
        sudo usermod -aG docker $USER
        rm get-docker.sh
        print_warning "You may need to log out and back in for Docker group changes to take effect"
    else
        print_success "Docker is already installed"
    fi
}

# Setup Seq container
setup_seq() {
    print_status "Setting up Seq container with persistent storage..."
    
    # Create persistent data directory
    sudo mkdir -p $SEQ_DATA_DIR
    sudo chown 5341:5341 $SEQ_DATA_DIR
    
    # Stop and remove existing Seq container if it exists
    docker stop refactor-mcp-seq 2>/dev/null || true
    docker rm refactor-mcp-seq 2>/dev/null || true
    
    # Create Seq container with restart policy
    docker run -d \
        --name refactor-mcp-seq \
        --restart unless-stopped \
        -p 5341:5341 \
        -v $SEQ_DATA_DIR:/data \
        -e ACCEPT_EULA=Y \
        -e SEQ_FIRSTRUN_ADMINPASSWORDHASH='' \
        datalust/seq:latest
    
    print_success "Seq container created and started"
    print_status "Seq will be available at: http://localhost:5341"
}

# Build RefactorMCP Web application
build_app() {
    print_status "Building RefactorMCP Web application as self-contained..."
    
    # Detect architecture
    ARCH=$(uname -m)
    case $ARCH in
        x86_64) RID="linux-x64" ;;
        aarch64) RID="linux-arm64" ;;
        armv7l) RID="linux-arm" ;;
        *) RID="linux-x64"; print_warning "Unknown architecture $ARCH, defaulting to linux-x64" ;;
    esac
    
    print_status "Building for runtime identifier: $RID"
    
    # Build self-contained application
    dotnet publish RefactorMCP.Web/RefactorMCP.Web.csproj \
        --configuration Release \
        --runtime $RID \
        --self-contained true \
        --output ./publish-web \
        /p:PublishSingleFile=true \
        /p:PublishTrimmed=true \
        /p:TrimMode=partial \
        /p:IncludeNativeLibrariesForSelfExtract=true
    
    # Create application directory
    sudo mkdir -p $REFACTOR_MCP_DIR
    sudo cp -r ./publish-web/* $REFACTOR_MCP_DIR/
    sudo chown -R $USER:$USER $REFACTOR_MCP_DIR
    sudo chmod +x $REFACTOR_MCP_DIR/RefactorMCP.Web
    
    print_success "Self-contained application built and deployed to $REFACTOR_MCP_DIR"
    print_success "No .NET runtime installation required on target machines!"
}

# Create systemd service for RefactorMCP
create_systemd_service() {
    print_status "Creating systemd service for RefactorMCP..."
    
    # Create service file
    sudo tee /etc/systemd/system/refactor-mcp.service > /dev/null <<EOF
[Unit]
Description=RefactorMCP Web Dashboard
Documentation=https://github.com/your-repo/refactor-mcp
After=network.target docker.service
Wants=docker.service

[Service]
Type=notify
ExecStart=$REFACTOR_MCP_DIR/RefactorMCP.Web
ExecReload=/bin/kill -s HUP \$MAINPID
WorkingDirectory=$REFACTOR_MCP_DIR
User=$USER
Group=$USER
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000;https://0.0.0.0:5001
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
KillMode=mixed
Restart=always
RestartSec=10
SyslogIdentifier=refactor-mcp
TimeoutStopSec=10

# Security settings
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=$REFACTOR_MCP_DIR
ReadWritePaths=/tmp
ReadWritePaths=/var/tmp

[Install]
WantedBy=multi-user.target
EOF

    # Create startup delay service (waits for Seq to be ready)
    sudo tee /etc/systemd/system/refactor-mcp-delayed.service > /dev/null <<EOF
[Unit]
Description=RefactorMCP Delayed Startup
After=refactor-mcp.service

[Service]
Type=oneshot
ExecStartPre=/bin/sleep 30
ExecStart=/bin/systemctl restart refactor-mcp.service
RemainAfterExit=true

[Install]
WantedBy=multi-user.target
EOF

    # Reload systemd and enable services
    sudo systemctl daemon-reload
    sudo systemctl enable refactor-mcp.service
    sudo systemctl enable refactor-mcp-delayed.service
    
    print_success "Systemd services created and enabled"
}

# Create management script
create_management_script() {
    print_status "Creating management script..."
    
    sudo tee /usr/local/bin/refactor-mcp-ctl > /dev/null <<'EOF'
#!/bin/bash

# RefactorMCP Control Script

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
    update)
        echo "Updating RefactorMCP application..."
        cd /opt/refactor-mcp-source 2>/dev/null || cd $(dirname $(find /home -name "RefactorMCP.Web.csproj" 2>/dev/null | head -1))
        if [ $? -eq 0 ]; then
            # Detect architecture for self-contained build
            ARCH=$(uname -m)
            case $ARCH in
                x86_64) RID="linux-x64" ;;
                aarch64) RID="linux-arm64" ;;
                armv7l) RID="linux-arm" ;;
                *) RID="linux-x64" ;;
            esac
            
            dotnet publish RefactorMCP.Web/RefactorMCP.Web.csproj \
                --configuration Release \
                --runtime $RID \
                --self-contained true \
                --output /tmp/refactor-mcp-update \
                /p:PublishSingleFile=true \
                /p:PublishTrimmed=true \
                /p:TrimMode=partial \
                /p:IncludeNativeLibrariesForSelfExtract=true
                
            sudo systemctl stop refactor-mcp
            sudo cp -r /tmp/refactor-mcp-update/* /opt/refactor-mcp/
            sudo chmod +x /opt/refactor-mcp/RefactorMCP.Web
            sudo systemctl start refactor-mcp
            rm -rf /tmp/refactor-mcp-update
            echo "Self-contained update completed"
        else
            echo "Could not find source directory"
        fi
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|status|logs|seq-logs|update}"
        exit 1
        ;;
esac
EOF

    sudo chmod +x /usr/local/bin/refactor-mcp-ctl
    
    # Create source directory link for updates
    sudo ln -sf $CURRENT_DIR /opt/refactor-mcp-source
    
    print_success "Management script created at /usr/local/bin/refactor-mcp-ctl"
}

# Update appsettings for production
update_config() {
    print_status "Updating production configuration..."
    
    # Update appsettings.Production.json
    cat > $REFACTOR_MCP_DIR/appsettings.Production.json <<EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Extensions.Hosting": "Information",
      "System.Net.Http": "Warning"
    },
    "File": {
      "Path": "/var/log/refactor-mcp/refactor-mcp-.log"
    },
    "Seq": {
      "ServerUrl": "http://localhost:5341",
      "ApiKey": ""
    }
  },
  "AllowedHosts": "*",
  "RefactorMCP": {
    "DefaultSolutionPaths": [],
    "MaxConcurrentOperations": 5,
    "CacheExpirationMinutes": 30
  }
}
EOF

    # Create log directory
    sudo mkdir -p /var/log/refactor-mcp
    sudo chown $USER:$USER /var/log/refactor-mcp
    
    print_success "Production configuration updated"
}

# Clean up Podman traces
cleanup_podman() {
    print_status "Cleaning up Podman environment traces..."
    
    # Unset DOCKER_HOST if it points to Podman
    if [[ "$DOCKER_HOST" == *"podman"* ]]; then
        print_warning "Found DOCKER_HOST pointing to Podman socket, unsetting it"
        unset DOCKER_HOST
        export DOCKER_HOST=""
    fi
    
    # Test Docker connection
    if docker ps >/dev/null 2>&1; then
        print_success "Docker is working correctly"
    else
        print_error "Docker connection failed even after cleanup"
        exit 1
    fi
}

# Main execution
main() {
    print_status "Starting RefactorMCP services setup..."
    
    check_sudo
    cleanup_podman
    install_docker
    setup_seq
    build_app
    update_config
    create_systemd_service
    create_management_script
    
    print_success "Setup completed!"
    echo ""
    echo "🎉 RefactorMCP Services are now installed!"
    echo ""
    echo "📋 Available commands:"
    echo "   refactor-mcp-ctl start     - Start all services"
    echo "   refactor-mcp-ctl stop      - Stop all services"  
    echo "   refactor-mcp-ctl restart   - Restart all services"
    echo "   refactor-mcp-ctl status    - Show service status"
    echo "   refactor-mcp-ctl logs      - Show RefactorMCP logs"
    echo "   refactor-mcp-ctl seq-logs  - Show Seq container logs"
    echo "   refactor-mcp-ctl update    - Update application"
    echo ""
    echo "🌐 Services will be available at:"
    echo "   RefactorMCP Dashboard: http://localhost:5000 (HTTP) / https://localhost:5001 (HTTPS)"
    echo "   Seq Logging: http://localhost:5341"
    echo "   Metrics: http://localhost:5000/metrics"
    echo "   Health: http://localhost:5000/health"
    echo ""
    echo "🚀 Starting services now..."
    refactor-mcp-ctl start
    
    print_success "All services started successfully!"
}

# Run main function
main "$@"
EOF