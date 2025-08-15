#!/bin/bash
# Start Services Manual Script
# Location: scripts/testing/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🚀 Manual RefactorMCP Services Startup"
echo ""
echo "This script will show you the commands to run with sudo."
echo "Copy and paste each command when ready:"
echo ""

echo "1. Clean up Docker environment:"
echo "   unset DOCKER_HOST"
echo "   export DOCKER_HOST=''"
echo ""

echo "2. Start Seq container:"
echo "   docker start refactor-mcp-seq || docker run -d --name refactor-mcp-seq --restart unless-stopped -p 5341:5341 -v /opt/seq/data:/data -e ACCEPT_EULA=Y datalust/seq:latest"
echo ""

echo "3. Fix and start RefactorMCP service:"
echo "   sudo systemctl stop refactor-mcp"
echo "   sudo cp /tmp/refactor-mcp.service /etc/systemd/system/refactor-mcp.service"
echo "   sudo systemctl daemon-reload"
echo "   sudo systemctl enable refactor-mcp"
echo "   sudo systemctl start refactor-mcp"
echo ""

echo "4. Check status:"
echo "   sudo systemctl status refactor-mcp --no-pager"
echo ""

echo "5. Test services:"
echo "   curl http://localhost:5000/health"
echo "   curl http://localhost:5000/api/mcp/server-info"