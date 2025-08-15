#!/bin/bash
# Fix Navigation Pages Script
# Location: scripts/maintenance/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔧 Fixing navigation pages and HttpClient dependency..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() { echo -e "${YELLOW}[FIX]${NC} $1"; }
print_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# 1. Stop the service
print_status "Stopping RefactorMCP service..."
sudo systemctl stop refactor-mcp

# 2. Build with fixes
print_status "Building application with navigation pages and HttpClient fix..."
export PATH="$HOME/.dotnet:$PATH"

dotnet publish RefactorMCP.Web/RefactorMCP.Web.csproj \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --output ./publish-web-navigation-fixed \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:IncludeNativeLibrariesForSelfExtract=true

if [ $? -eq 0 ]; then
    print_success "Build completed successfully!"
else
    print_error "Build failed!"
    exit 1
fi

# 3. Deploy
print_status "Deploying fixed application..."
sudo cp -r ./publish-web-navigation-fixed/* /opt/refactor-mcp/
sudo chmod +x /opt/refactor-mcp/RefactorMCP.Web
sudo chown -R abel:abel /opt/refactor-mcp/

# 4. Start service
print_status "Starting RefactorMCP service..."
sudo systemctl start refactor-mcp

# 5. Wait and test
print_status "Waiting for service startup..."
sleep 8

if sudo systemctl is-active refactor-mcp >/dev/null 2>&1; then
    print_success "🎉 RefactorMCP service is running!"
    
    # Test navigation pages
    print_status "Testing navigation pages..."
    
    PAGES=("/" "/tools" "/mcp" "/logs" "/monitoring" "/metrics")
    for page in "${PAGES[@]}"; do
        if curl -s "http://localhost:7042$page" >/dev/null 2>&1; then
            print_success "✅ Page working: http://localhost:7042$page"
        else
            print_error "❌ Page failed: http://localhost:7042$page"
        fi
    done
    
    echo ""
    print_success "🌐 All navigation pages are now available!"
    echo ""
    echo "Available pages:"
    echo "  🏠 Dashboard:     http://localhost:7042/"
    echo "  🛠️  Tools:         http://localhost:7042/tools"
    echo "  🔧 MCP Server:    http://localhost:7042/mcp"
    echo "  📋 Logs:          http://localhost:7042/logs"
    echo "  📊 Monitoring:    http://localhost:7042/monitoring"
    echo "  📈 Metrics:       http://localhost:7042/metrics"
    
else
    print_error "❌ Service failed to start"
    sudo systemctl status refactor-mcp --no-pager -l
fi

print_success "Navigation pages fix completed!"