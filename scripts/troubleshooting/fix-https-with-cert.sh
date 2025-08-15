#!/bin/bash
# Fix Https With Cert Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔧 Fixing HTTPS with existing certificate..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[FIX]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

HTTP_PORT=7042
HTTPS_PORT=7043

# 1. Stop current service
print_status "Stopping RefactorMCP service..."
sudo systemctl stop refactor-mcp 2>/dev/null || true
pkill -f "RefactorMCP.Web" 2>/dev/null || true
sleep 2

# 2. Trust the certificate for the current user
print_status "Ensuring certificate is trusted..."
dotnet dev-certs https --trust 2>/dev/null || echo "Certificate trust attempted"

# 3. Check certificate location and copy to system location
print_status "Checking certificate accessibility..."

# Get user's certificate store
CERT_STORE="$HOME/.dotnet/corefx/cryptography/x509stores/my"
if [ -d "$CERT_STORE" ]; then
    print_status "Found user certificate store: $CERT_STORE"
    ls -la "$CERT_STORE/" 2>/dev/null || echo "No certificates found"
fi

# 4. Update systemd service to use Development environment (which can access the cert)
print_status "Updating service to use Development environment with HTTPS..."
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
Environment=ASPNETCORE_ENVIRONMENT=Development
Environment=ASPNETCORE_URLS=http://0.0.0.0:$HTTP_PORT;https://0.0.0.0:$HTTPS_PORT
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=HOME=/home/abel
Restart=always
RestartSec=10
SyslogIdentifier=refactor-mcp

# Security settings (relaxed for Development)
NoNewPrivileges=true
PrivateTmp=false
ProtectSystem=strict
ProtectHome=false
ReadWritePaths=/opt/refactor-mcp
ReadWritePaths=/tmp
ReadWritePaths=/var/tmp
ReadWritePaths=/var/log/refactor-mcp
ReadWritePaths=/home/abel/.dotnet

[Install]
WantedBy=multi-user.target
EOF

print_success "Service updated to Development environment with HTTPS support"

# 5. Alternative: Create HTTP-only fallback service
print_status "Creating HTTP-only fallback service configuration..."
sudo tee /etc/systemd/system/refactor-mcp-http.service > /dev/null <<EOF
[Unit]
Description=RefactorMCP Web Dashboard (HTTP Only)
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
SyslogIdentifier=refactor-mcp-http

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

# 6. Test manual HTTPS first
print_status "Testing manual HTTPS execution..."
cd /opt/refactor-mcp

export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:$HTTP_PORT;https://localhost:$HTTPS_PORT"
export HOME=/home/abel

print_status "Quick HTTPS test (10 seconds)..."
timeout 10s sudo -u abel -E ./RefactorMCP.Web 2>&1 &
TEST_PID=$!

sleep 5

# Check HTTP and HTTPS
HTTP_WORKS=false
HTTPS_WORKS=false

if curl -s http://localhost:$HTTP_PORT/health >/dev/null 2>&1; then
    print_success "✅ HTTP endpoint working: http://localhost:$HTTP_PORT/health"
    HTTP_WORKS=true
fi

if curl -k -s https://localhost:$HTTPS_PORT/health >/dev/null 2>&1; then
    print_success "✅ HTTPS endpoint working: https://localhost:$HTTPS_PORT/health"
    HTTPS_WORKS=true
fi

# Kill test process
kill $TEST_PID 2>/dev/null || true
wait $TEST_PID 2>/dev/null || true

# 7. Choose which service to use
print_status "Reloading systemd configuration..."
sudo systemctl daemon-reload

if [ "$HTTPS_WORKS" = true ]; then
    print_success "HTTPS works! Using main service with HTTPS support..."
    sudo systemctl enable refactor-mcp
    sudo systemctl start refactor-mcp
    SERVICE_NAME="refactor-mcp"
else
    print_warning "HTTPS not working, using HTTP-only service..."
    sudo systemctl enable refactor-mcp-http
    sudo systemctl start refactor-mcp-http
    SERVICE_NAME="refactor-mcp-http"
fi

# 8. Wait and test service
print_status "Waiting for $SERVICE_NAME service startup..."
sleep 5

if sudo systemctl is-active $SERVICE_NAME >/dev/null 2>&1; then
    print_success "🎉 RefactorMCP service started successfully!"
    
    # Test endpoints
    print_status "Testing service endpoints..."
    
    for i in {1..10}; do
        if curl -s http://localhost:$HTTP_PORT/health >/dev/null 2>&1; then
            print_success "✅ HTTP Health: http://localhost:$HTTP_PORT/health"
            echo "Response: $(curl -s http://localhost:$HTTP_PORT/health)"
            break
        fi
        echo -n "."
        sleep 2
    done
    
    if [ "$HTTPS_WORKS" = true ]; then
        if curl -k -s https://localhost:$HTTPS_PORT/health >/dev/null 2>&1; then
            print_success "✅ HTTPS Health: https://localhost:$HTTPS_PORT/health"
        else
            print_warning "❌ HTTPS not accessible via service"
        fi
    fi
    
    echo ""
    
    # Test MCP API
    if curl -s http://localhost:$HTTP_PORT/api/mcp/server-info >/dev/null 2>&1; then
        print_success "✅ MCP API: http://localhost:$HTTP_PORT/api/mcp/server-info"
    else
        print_error "❌ MCP API not responding"
    fi
    
    echo ""
    print_success "🌐 RefactorMCP is now running!"
    echo ""
    echo "Available endpoints:"
    echo "  📊 Dashboard:  http://localhost:$HTTP_PORT"
    if [ "$HTTPS_WORKS" = true ]; then
        echo "  🔒 Dashboard:  https://localhost:$HTTPS_PORT"
    fi
    echo "  ❤️  Health:     http://localhost:$HTTP_PORT/health"
    echo "  🔧 MCP API:    http://localhost:$HTTP_PORT/api/mcp/server-info"
    echo "  🛠️  Tools:      http://localhost:$HTTP_PORT/api/mcp/tools"
    echo "  📈 Metrics:    http://localhost:$HTTP_PORT/metrics"
    echo "  📋 Seq Logs:   http://localhost:5341"
    
else
    print_error "❌ Service failed to start"
    
    echo ""
    echo "=== Service Status ==="
    sudo systemctl status $SERVICE_NAME --no-pager -l
    
    echo ""
    echo "=== Recent Logs ==="
    sudo journalctl -u $SERVICE_NAME --no-pager -l -n 10
fi

print_success "HTTPS certificate configuration completed!"