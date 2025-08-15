#!/bin/bash
# Fix Json Serialization Script
# Location: scripts/maintenance/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔧 Fixing JSON serialization issues in MCP API..."

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

# 2. Build the application with JSON serialization fixes
print_status "Building application with JSON serialization fixes..."
export PATH="$HOME/.dotnet:$PATH"

dotnet publish RefactorMCP.Web/RefactorMCP.Web.csproj \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --output ./publish-web-json-fixed \
    /p:PublishSingleFile=true \
    /p:PublishTrimmed=false \
    /p:IncludeNativeLibrariesForSelfExtract=true

if [ $? -eq 0 ]; then
    print_success "Build completed successfully!"
else
    print_error "Build failed!"
    exit 1
fi

# 3. Deploy the fixed application
print_status "Deploying fixed application..."
sudo cp -r ./publish-web-json-fixed/* /opt/refactor-mcp/
sudo chmod +x /opt/refactor-mcp/RefactorMCP.Web
sudo chown -R abel:abel /opt/refactor-mcp/

print_success "Application deployed"

# 4. Start Seq container if not running
print_status "Checking Seq container..."
if ! docker ps --format "{{.Names}}" | grep -q refactor-mcp-seq; then
    print_status "Starting Seq container..."
    sudo mkdir -p /opt/seq/data
    sudo chown 5341:5341 /opt/seq/data
    
    docker run -d \
        --name refactor-mcp-seq \
        --restart unless-stopped \
        -p 5341:5341 \
        -v /opt/seq/data:/data \
        -e ACCEPT_EULA=Y \
        datalust/seq:latest
        
    print_success "Seq container started"
    sleep 5
else
    print_success "Seq container is already running"
fi

# 5. Start the service
print_status "Starting RefactorMCP service..."
sudo systemctl start refactor-mcp

# 6. Wait for startup
print_status "Waiting for service to start..."
sleep 10

# 7. Test endpoints
if sudo systemctl is-active refactor-mcp >/dev/null 2>&1; then
    print_success "🎉 RefactorMCP service is running!"
    
    # Test health endpoint
    print_status "Testing health endpoint..."
    if curl -s http://localhost:7042/health >/dev/null 2>&1; then
        print_success "✅ Health: http://localhost:7042/health"
        echo "Response: $(curl -s http://localhost:7042/health)"
    else
        print_error "❌ Health endpoint not responding"
    fi
    
    echo ""
    
    # Test MCP server info
    print_status "Testing MCP server info..."
    if curl -s http://localhost:7042/api/mcp/server-info >/dev/null 2>&1; then
        print_success "✅ MCP Server Info: http://localhost:7042/api/mcp/server-info"
        echo "Response:"
        curl -s http://localhost:7042/api/mcp/server-info | jq . 2>/dev/null || curl -s http://localhost:7042/api/mcp/server-info
    else
        print_error "❌ MCP server info endpoint failed"
        echo "Error response:"
        curl -s http://localhost:7042/api/mcp/server-info || echo "No response"
    fi
    
    echo ""
    
    # Test MCP tools list
    print_status "Testing MCP tools list..."
    if curl -s http://localhost:7042/api/mcp/tools >/dev/null 2>&1; then
        print_success "✅ MCP Tools: http://localhost:7042/api/mcp/tools"
        echo "First few tools:"
        curl -s http://localhost:7042/api/mcp/tools | jq '.tools[0:3]' 2>/dev/null || curl -s http://localhost:7042/api/mcp/tools | head -10
    else
        print_error "❌ MCP tools endpoint failed"
        echo "Error response:"
        curl -s http://localhost:7042/api/mcp/tools || echo "No response"
    fi
    
    echo ""
    
    # Test Seq
    print_status "Testing Seq endpoint..."
    if curl -s http://localhost:5341 >/dev/null 2>&1; then
        print_success "✅ Seq: http://localhost:5341"
    else
        print_error "❌ Seq not accessible"
    fi
    
    echo ""
    print_success "🌐 RefactorMCP with HTTP MCP API is working!"
    echo ""
    echo "Available endpoints:"
    echo "  📊 Dashboard:     http://localhost:7042"
    echo "  ❤️  Health:       http://localhost:7042/health"
    echo "  🔧 MCP Server:    http://localhost:7042/api/mcp/server-info"
    echo "  🛠️  MCP Tools:     http://localhost:7042/api/mcp/tools"
    echo "  📈 Metrics:       http://localhost:7042/metrics"
    echo "  📋 Seq Logs:      http://localhost:5341"
    echo ""
    echo "🧪 Test MCP API:"
    echo "  curl http://localhost:7042/api/mcp/server-info"
    echo "  curl http://localhost:7042/api/mcp/tools"
    
else
    print_error "❌ Service failed to start"
    
    echo ""
    echo "=== Service Status ==="
    sudo systemctl status refactor-mcp --no-pager -l
    
    echo ""
    echo "=== Recent Logs ==="
    sudo journalctl -u refactor-mcp --no-pager -l -n 15
fi

print_success "JSON serialization fix completed!"