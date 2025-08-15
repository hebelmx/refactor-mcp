#!/bin/bash
# Fix Port Conflict Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔧 Fixing RefactorMCP port conflict..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[FIX]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# 1. Stop any running RefactorMCP processes
print_status "Stopping all RefactorMCP processes..."
pkill -f "RefactorMCP.Web" 2>/dev/null || true
sleep 2

# 2. Stop the systemd service if running
print_status "Stopping systemd service..."
sudo systemctl stop refactor-mcp 2>/dev/null || true

# 3. Check if ports are free now
print_status "Checking port availability..."
if netstat -tlnp | grep -q ":5000"; then
    print_error "Port 5000 still in use:"
    netstat -tlnp | grep ":5000"
    
    # Force kill any remaining processes
    print_status "Force killing processes on port 5000..."
    sudo fuser -k 5000/tcp 2>/dev/null || true
    sleep 2
fi

if netstat -tlnp | grep -q ":5001"; then
    print_error "Port 5001 still in use:"
    netstat -tlnp | grep ":5001"
    
    # Force kill any remaining processes
    print_status "Force killing processes on port 5001..."
    sudo fuser -k 5001/tcp 2>/dev/null || true
    sleep 2
fi

# 4. Verify ports are free
if ! netstat -tlnp | grep -q -E ":(5000|5001)"; then
    print_success "Ports 5000 and 5001 are now free"
else
    print_error "Some ports are still in use:"
    netstat -tlnp | grep -E ":(5000|5001)"
fi

# 5. Start the service cleanly
print_status "Starting RefactorMCP service..."
sudo systemctl start refactor-mcp

# 6. Wait for startup
print_status "Waiting for service to start..."
sleep 5

# 7. Check service status
if sudo systemctl is-active refactor-mcp >/dev/null 2>&1; then
    print_success "🎉 RefactorMCP service is running!"
    
    # Show service status
    echo ""
    echo "=== Service Status ==="
    sudo systemctl status refactor-mcp --no-pager -l
    
    # Test endpoints
    echo ""
    echo "=== Testing Endpoints ==="
    
    for i in {1..10}; do
        if curl -s http://localhost:5000/health >/dev/null 2>&1; then
            print_success "✅ Health endpoint responding: http://localhost:5000/health"
            break
        fi
        sleep 2
        echo -n "."
    done
    
    echo ""
    
    # Test MCP API
    if curl -s http://localhost:5000/api/mcp/server-info >/dev/null 2>&1; then
        print_success "✅ MCP API responding: http://localhost:5000/api/mcp/server-info"
    else
        print_error "❌ MCP API not responding"
    fi
    
    echo ""
    echo "🌐 RefactorMCP is now available at:"
    echo "   Dashboard: http://localhost:5000"
    echo "   Health: http://localhost:5000/health"
    echo "   MCP API: http://localhost:5000/api/mcp/server-info"
    echo "   Tools: http://localhost:5000/api/mcp/tools"
    echo "   Seq Logs: http://localhost:5341"
    
else
    print_error "❌ Service failed to start"
    
    echo ""
    echo "=== Service Status ==="
    sudo systemctl status refactor-mcp --no-pager -l
    
    echo ""
    echo "=== Recent Logs ==="
    sudo journalctl -u refactor-mcp --no-pager -l -n 10
fi

print_success "Port conflict fix completed!"