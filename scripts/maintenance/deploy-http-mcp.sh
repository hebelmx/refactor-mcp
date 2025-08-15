#!/bin/bash
# Deploy Http Mcp Script
# Location: scripts/maintenance/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🚀 Deploying RefactorMCP Web with HTTP MCP API..."

# Stop existing service
echo "Stopping existing service..."
sudo systemctl stop refactor-mcp

# Update the application
echo "Updating application files..."
sudo cp -r ./publish-web-updated/* /opt/refactor-mcp/
sudo chmod +x /opt/refactor-mcp/RefactorMCP.Web

# Start the service
echo "Starting updated service..."
sudo systemctl start refactor-mcp

# Check status
echo ""
echo "📊 Service Status:"
sudo systemctl status refactor-mcp --no-pager -l

echo ""
echo "✅ HTTP MCP API has been deployed!"
echo ""
echo "🌐 Available endpoints:"
echo "   Web Dashboard: http://localhost:5000"
echo "   MCP HTTP API:  http://localhost:5000/api/mcp"
echo "   Server Info:   http://localhost:5000/api/mcp/server-info"
echo "   List Tools:    http://localhost:5000/api/mcp/tools"
echo "   Health Check:  http://localhost:5000/health"
echo "   Metrics:       http://localhost:5000/metrics"
echo "   Seq Logs:      http://localhost:5341"
echo ""
echo "🔧 Test commands:"
echo "   curl http://localhost:5000/api/mcp/server-info"
echo "   curl http://localhost:5000/api/mcp/tools"