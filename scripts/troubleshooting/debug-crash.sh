#!/bin/bash
# Debug Crash Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🐛 Debugging RefactorMCP application crash..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_status() { echo -e "${BLUE}[DEBUG]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }

# 1. Check if application exists and is executable
print_status "Checking application file..."
if [ -f "/opt/refactor-mcp/RefactorMCP.Web" ]; then
    ls -la /opt/refactor-mcp/RefactorMCP.Web
    print_success "Application file exists"
    
    if [ -x "/opt/refactor-mcp/RefactorMCP.Web" ]; then
        print_success "Application is executable"
    else
        print_error "Application is not executable"
        echo "Run: sudo chmod +x /opt/refactor-mcp/RefactorMCP.Web"
    fi
else
    print_error "Application file not found at /opt/refactor-mcp/RefactorMCP.Web"
    print_status "Checking if we need to deploy it..."
    
    if [ -d "./publish-web-updated" ]; then
        print_status "Found publish-web-updated directory, deploying..."
        sudo cp -r ./publish-web-updated/* /opt/refactor-mcp/
        sudo chmod +x /opt/refactor-mcp/RefactorMCP.Web
        sudo chown -R abel:abel /opt/refactor-mcp/
        print_success "Application deployed"
    else
        print_error "No application to deploy. Need to build first."
        exit 1
    fi
fi

# 2. Check application dependencies
print_status "Checking application dependencies..."
cd /opt/refactor-mcp

# Test if it can show help or version
echo "Testing basic execution..."
timeout 5s ./RefactorMCP.Web --help 2>&1 || echo "Help command failed or timed out"
echo ""

timeout 5s ./RefactorMCP.Web --version 2>&1 || echo "Version command failed or timed out"
echo ""

# 3. Check .NET runtime dependencies
print_status "Checking .NET dependencies..."
ldd /opt/refactor-mcp/RefactorMCP.Web 2>/dev/null | head -10 || echo "ldd failed - might be self-contained"

# 4. Try to run manually with more verbose output
print_status "Testing manual execution with verbose logging..."
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:5000"
export DOTNET_PRINT_TELEMETRY_MESSAGE=false

echo "Starting application manually (will stop after 10 seconds)..."
timeout 10s ./RefactorMCP.Web 2>&1 &
APP_PID=$!

# Wait a bit then check if it's responding
sleep 3

# Test if the web server is responding
if curl -s http://localhost:5000/health >/dev/null 2>&1; then
    print_success "✅ Application started successfully and is responding!"
    curl -s http://localhost:5000/health
else
    print_warning "❌ Application not responding to HTTP requests"
fi

# Kill the test process
kill $APP_PID 2>/dev/null || true
wait $APP_PID 2>/dev/null || true

# 5. Check system resources
print_status "Checking system resources..."
echo "Memory usage:"
free -h

echo ""
echo "Disk space:"
df -h /opt

echo ""
echo "Process limits:"
ulimit -a | grep -E "(open files|max user processes|virtual memory)"

# 6. Check for missing shared libraries or dependencies
print_status "Checking for missing dependencies..."
if command -v strace >/dev/null 2>&1; then
    print_status "Running strace to check for missing files (first 3 seconds)..."
    timeout 3s strace -e trace=openat ./RefactorMCP.Web 2>&1 | grep -E "ENOENT|No such file" | head -10 || true
else
    print_warning "strace not available for detailed debugging"
fi

# 7. Check logs directory permissions
print_status "Checking log directory permissions..."
if [ -d "/var/log/refactor-mcp" ]; then
    ls -la /var/log/refactor-mcp/
else
    print_warning "Log directory doesn't exist, creating it..."
    sudo mkdir -p /var/log/refactor-mcp
    sudo chown abel:abel /var/log/refactor-mcp
fi

# 8. Suggest next steps
print_status "Next debugging steps:"
echo ""
echo "1. Try running manually with your user:"
echo "   cd /opt/refactor-mcp"
echo "   ./RefactorMCP.Web"
echo ""
echo "2. Check detailed service logs:"
echo "   sudo journalctl -u refactor-mcp -f"
echo ""
echo "3. If manual execution works, try starting service:"
echo "   sudo systemctl start refactor-mcp"
echo ""
echo "4. Check if ports are available:"
echo "   netstat -tlnp | grep -E ':(5000|5001)'"

print_status "Debug information collected!"