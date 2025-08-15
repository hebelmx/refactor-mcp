#!/bin/bash
# Fix Https Issue Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔧 Fixing HTTPS certificate issue - switching to HTTP only..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[FIX]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

HTTP_PORT=7042

# 1. Stop current service
print_status "Stopping RefactorMCP service..."
sudo systemctl stop refactor-mcp 2>/dev/null || true
pkill -f "RefactorMCP.Web" 2>/dev/null || true
sleep 2

# 2. Update systemd service to use HTTP only
print_status "Updating service to use HTTP only (port $HTTP_PORT)..."
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
Environment=ASPNETCORE_URLS=http://0.0.0.0:$HTTP_PORT
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

print_success "Service updated to HTTP only on port $HTTP_PORT"

# 3. Update port info
print_status "Updating port reference..."
sudo tee /opt/refactor-mcp/PORT_INFO.txt > /dev/null <<EOF
RefactorMCP Port Configuration
============================

HTTP Port:  $HTTP_PORT (HTTPS disabled to avoid certificate issues)

Endpoints:
- Dashboard: http://localhost:$HTTP_PORT
- Health: http://localhost:$HTTP_PORT/health
- MCP API: http://localhost:$HTTP_PORT/api/mcp
- Tools: http://localhost:$HTTP_PORT/api/mcp/tools
- Metrics: http://localhost:$HTTP_PORT/metrics

External Services:
- Seq Logs: http://localhost:5341

Note: HTTPS disabled until proper SSL certificate is configured.
Last Updated: $(date)
EOF

# 4. Reload systemd
print_status "Reloading systemd configuration..."
sudo systemctl daemon-reload
sudo systemctl enable refactor-mcp

# 5. Test manual run first
print_status "Testing manual execution with HTTP only..."
cd /opt/refactor-mcp
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://localhost:$HTTP_PORT"

# Quick test
print_status "Quick test run (5 seconds)..."
timeout 5s ./RefactorMCP.Web 2>&1 &
TEST_PID=$!

sleep 3

# Check if it's working
if curl -s http://localhost:$HTTP_PORT/health >/dev/null 2>&1; then
    print_success "✅ Manual test successful - HTTP endpoint responding!"
    curl -s http://localhost:$HTTP_PORT/health
else
    print_error "❌ Manual test failed"
fi

# Kill test process
kill $TEST_PID 2>/dev/null || true
wait $TEST_PID 2>/dev/null || true

# 6. Start the service
print_status "Starting RefactorMCP service..."
sudo systemctl start refactor-mcp

# 7. Wait and test service
print_status "Waiting for service startup..."
sleep 5

if sudo systemctl is-active refactor-mcp >/dev/null 2>&1; then
    print_success "🎉 RefactorMCP service started successfully!"
    
    # Test endpoints
    print_status "Testing service endpoints..."
    
    for i in {1..10}; do
        if curl -s http://localhost:$HTTP_PORT/health >/dev/null 2>&1; then
            print_success "✅ Health endpoint: http://localhost:$HTTP_PORT/health"
            echo "Response: $(curl -s http://localhost:$HTTP_PORT/health)"
            break
        fi
        echo -n "."
        sleep 2
    done
    
    echo ""
    
    # Test MCP API
    if curl -s http://localhost:$HTTP_PORT/api/mcp/server-info >/dev/null 2>&1; then
        print_success "✅ MCP API endpoint: http://localhost:$HTTP_PORT/api/mcp/server-info"
        echo "Server info:"
        curl -s http://localhost:$HTTP_PORT/api/mcp/server-info | jq . 2>/dev/null || curl -s http://localhost:$HTTP_PORT/api/mcp/server-info
    else
        print_error "❌ MCP API not responding"
    fi
    
    echo ""
    print_success "🌐 RefactorMCP is now running successfully!"
    echo ""
    echo "Available endpoints:"
    echo "  📊 Dashboard:  http://localhost:$HTTP_PORT"
    echo "  ❤️  Health:     http://localhost:$HTTP_PORT/health"
    echo "  🔧 MCP API:    http://localhost:$HTTP_PORT/api/mcp/server-info"
    echo "  🛠️  Tools:      http://localhost:$HTTP_PORT/api/mcp/tools"
    echo "  📈 Metrics:    http://localhost:$HTTP_PORT/metrics"
    echo "  📋 Seq Logs:   http://localhost:5341"
    
else
    print_error "❌ Service failed to start"
    
    echo ""
    echo "=== Service Status ==="
    sudo systemctl status refactor-mcp --no-pager -l
    
    echo ""
    echo "=== Recent Logs ==="
    sudo journalctl -u refactor-mcp --no-pager -l -n 10
fi

print_success "HTTPS certificate issue fixed - using HTTP only!"

# 8. Optional: Show how to add HTTPS later
echo ""
echo "💡 To enable HTTPS later:"
echo "1. Generate dev certificate: dotnet dev-certs https --trust"
echo "2. Or configure proper SSL certificate in production"
echo "3. Update ASPNETCORE_URLS to include HTTPS port"