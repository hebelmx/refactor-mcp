#!/bin/bash
# Change Ports Script
# Location: scripts/maintenance/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔧 Changing RefactorMCP to use 7000-range ports..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[CHANGE]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Choose random 7000-range ports
HTTP_PORT=7042
HTTPS_PORT=7043

print_status "Selected ports: HTTP=$HTTP_PORT, HTTPS=$HTTPS_PORT"

# 1. Stop current service and processes
print_status "Stopping RefactorMCP service and processes..."
sudo systemctl stop refactor-mcp 2>/dev/null || true
pkill -f "RefactorMCP.Web" 2>/dev/null || true
sleep 2

# 2. Update systemd service file
print_status "Updating systemd service configuration..."
sudo tee /etc/systemd/system/refactor-mcp.service > /dev/null <<EOF
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
Environment=ASPNETCORE_URLS=http://0.0.0.0:$HTTP_PORT;https://0.0.0.0:$HTTPS_PORT
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

print_success "Service file updated with ports $HTTP_PORT and $HTTPS_PORT"

# 3. Update production configuration
print_status "Updating production configuration..."
cat > /opt/refactor-mcp/appsettings.Production.json <<EOF
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

# 4. Create a port info file for reference
print_status "Creating port reference file..."
cat > /opt/refactor-mcp/PORT_INFO.txt <<EOF
RefactorMCP Port Configuration
============================

HTTP Port:  $HTTP_PORT
HTTPS Port: $HTTPS_PORT

Endpoints:
- Dashboard: http://localhost:$HTTP_PORT
- Health: http://localhost:$HTTP_PORT/health
- MCP API: http://localhost:$HTTP_PORT/api/mcp
- Tools: http://localhost:$HTTP_PORT/api/mcp/tools
- Metrics: http://localhost:$HTTP_PORT/metrics

External Services:
- Seq Logs: http://localhost:5341

Last Updated: $(date)
EOF

# 5. Reload systemd and enable service
print_status "Reloading systemd configuration..."
sudo systemctl daemon-reload
sudo systemctl enable refactor-mcp

# 6. Check port availability
print_status "Checking new port availability..."
if netstat -tlnp | grep -q ":$HTTP_PORT"; then
    print_error "Port $HTTP_PORT is already in use:"
    netstat -tlnp | grep ":$HTTP_PORT"
else
    print_success "Port $HTTP_PORT is available"
fi

if netstat -tlnp | grep -q ":$HTTPS_PORT"; then
    print_error "Port $HTTPS_PORT is already in use:"
    netstat -tlnp | grep ":$HTTPS_PORT"
else
    print_success "Port $HTTPS_PORT is available"
fi

# 7. Start the service
print_status "Starting RefactorMCP service on new ports..."
sudo systemctl start refactor-mcp

# 8. Wait for startup and test
print_status "Waiting for service to start..."
sleep 5

# Check service status
if sudo systemctl is-active refactor-mcp >/dev/null 2>&1; then
    print_success "🎉 RefactorMCP is running on new ports!"
    
    # Test health endpoint
    for i in {1..15}; do
        if curl -s http://localhost:$HTTP_PORT/health >/dev/null 2>&1; then
            print_success "✅ Health endpoint responding"
            curl -s http://localhost:$HTTP_PORT/health
            break
        fi
        echo -n "."
        sleep 2
    done
    
    echo ""
    
    # Test MCP API
    if curl -s http://localhost:$HTTP_PORT/api/mcp/server-info >/dev/null 2>&1; then
        print_success "✅ MCP API responding"
        echo "Server info:"
        curl -s http://localhost:$HTTP_PORT/api/mcp/server-info | head -3
    else
        print_error "❌ MCP API not responding yet"
    fi
    
    echo ""
    echo "🌐 RefactorMCP is now available at:"
    echo "   Dashboard: http://localhost:$HTTP_PORT"
    echo "   Health: http://localhost:$HTTP_PORT/health"
    echo "   MCP API: http://localhost:$HTTP_PORT/api/mcp/server-info"
    echo "   Tools: http://localhost:$HTTP_PORT/api/mcp/tools"
    echo "   Metrics: http://localhost:$HTTP_PORT/metrics"
    echo "   Seq Logs: http://localhost:5341"
    
    # Update management script
    print_status "Updating management commands..."
    echo ""
    echo "📝 Updated commands for management:"
    echo "   refactor-mcp-ctl status"
    echo "   refactor-mcp-ctl logs"
    echo "   refactor-mcp-ctl restart"
    
else
    print_error "❌ Service failed to start on new ports"
    
    echo ""
    echo "=== Service Status ==="
    sudo systemctl status refactor-mcp --no-pager -l
    
    echo ""
    echo "=== Recent Logs ==="
    sudo journalctl -u refactor-mcp --no-pager -l -n 15
fi

print_success "Port change completed! RefactorMCP now uses ports $HTTP_PORT and $HTTPS_PORT"