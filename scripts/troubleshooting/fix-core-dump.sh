#!/bin/bash
# Fix Core Dump Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔧 Fixing RefactorMCP service core dump issue..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[FIX]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Stop the failing service
print_status "Stopping RefactorMCP service..."
sudo systemctl stop refactor-mcp

# Check if the application exists
if [ ! -f "/opt/refactor-mcp/RefactorMCP.Web" ]; then
    print_error "RefactorMCP.Web application not found at /opt/refactor-mcp/RefactorMCP.Web"
    print_status "Deploying the updated application..."
    
    if [ -d "./publish-web-updated" ]; then
        sudo cp -r ./publish-web-updated/* /opt/refactor-mcp/
        sudo chmod +x /opt/refactor-mcp/RefactorMCP.Web
        print_success "Application deployed"
    else
        print_error "publish-web-updated directory not found. Please run the build first."
        exit 1
    fi
fi

# Create the corrected service file
print_status "Creating corrected systemd service file..."
sudo tee /etc/systemd/system/refactor-mcp.service > /dev/null <<'EOF'
[Unit]
Description=RefactorMCP Web Dashboard
Documentation=https://github.com/your-repo/refactor-mcp
After=network.target docker.service
Wants=docker.service

[Service]
Type=simple
ExecStart=/opt/refactor-mcp/RefactorMCP.Web
WorkingDirectory=/opt/refactor-mcp
User=abel
Group=abel
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000;https://0.0.0.0:5001
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Restart=always
RestartSec=10
SyslogIdentifier=refactor-mcp

# Security settings
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/opt/refactor-mcp
ReadWritePaths=/tmp
ReadWritePaths=/var/tmp
ReadWritePaths=/var/log/refactor-mcp

[Install]
WantedBy=multi-user.target
EOF

print_success "Service file updated with Type=simple"

# Create log directory
print_status "Creating log directory..."
sudo mkdir -p /var/log/refactor-mcp
sudo chown abel:abel /var/log/refactor-mcp

# Reload systemd
print_status "Reloading systemd daemon..."
sudo systemctl daemon-reload

# Enable service
sudo systemctl enable refactor-mcp

# Test the application manually first
print_status "Testing application manually..."
cd /opt/refactor-mcp
if sudo -u abel ./RefactorMCP.Web --version 2>/dev/null; then
    print_success "Application can start manually"
else
    print_status "Testing application startup (will timeout after 10 seconds)..."
    timeout 10s sudo -u abel ./RefactorMCP.Web &
    MANUAL_PID=$!
    sleep 5
    kill $MANUAL_PID 2>/dev/null || true
    wait $MANUAL_PID 2>/dev/null || true
    print_status "Manual test completed"
fi

# Start the service
print_status "Starting RefactorMCP service..."
sudo systemctl start refactor-mcp

# Wait a moment for startup
sleep 3

# Check status
print_status "Checking service status..."
if sudo systemctl is-active refactor-mcp >/dev/null 2>&1; then
    print_success "🎉 RefactorMCP service is running!"
    
    # Show status
    echo ""
    echo "=== Service Status ==="
    sudo systemctl status refactor-mcp --no-pager -l
    
    # Test HTTP endpoints
    echo ""
    echo "=== Testing HTTP Endpoints ==="
    
    # Wait for web server to be ready
    for i in {1..30}; do
        if curl -s http://localhost:5000/health >/dev/null 2>&1; then
            print_success "Health endpoint is responding"
            break
        fi
        sleep 1
        echo -n "."
    done
    
    echo ""
    echo "🌐 Available endpoints:"
    echo "   Dashboard: http://localhost:5000"
    echo "   Health: http://localhost:5000/health"
    echo "   MCP API: http://localhost:5000/api/mcp/server-info"
    echo "   Seq Logs: http://localhost:5341"
    
else
    print_error "❌ Service failed to start"
    
    echo ""
    echo "=== Service Status ==="
    sudo systemctl status refactor-mcp --no-pager -l
    
    echo ""
    echo "=== Recent Logs ==="
    sudo journalctl -u refactor-mcp --no-pager -l -n 20
    
    echo ""
    echo "🔍 Debugging steps:"
    echo "1. Check logs: sudo journalctl -u refactor-mcp -f"
    echo "2. Test manually: cd /opt/refactor-mcp && ./RefactorMCP.Web"
    echo "3. Check permissions: ls -la /opt/refactor-mcp/"
fi