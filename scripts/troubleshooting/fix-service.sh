#!/bin/bash
# Fix Service Script
# Location: scripts/troubleshooting/

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../common/colors.sh"
source "$SCRIPT_DIR/../common/utils.sh"


echo "🔧 Fixing RefactorMCP systemd service configuration..."

# Stop the service first
echo "Stopping refactor-mcp service..."
sudo systemctl stop refactor-mcp

# Update the service file with corrected configuration
echo "Updating service configuration..."
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

# Reload systemd configuration
echo "Reloading systemd configuration..."
sudo systemctl daemon-reload

# Enable the service
echo "Enabling service..."
sudo systemctl enable refactor-mcp

echo "✅ Service configuration fixed!"
echo ""
echo "🚀 Starting services now..."

# Start Seq container first
echo "Starting Seq container..."
docker start refactor-mcp-seq
sleep 5

# Start RefactorMCP service
echo "Starting RefactorMCP service..."
sudo systemctl start refactor-mcp

# Check status
echo ""
echo "📊 Service Status:"
sudo systemctl status refactor-mcp --no-pager -l

echo ""
echo "🌐 If successful, RefactorMCP will be available at:"
echo "   Dashboard: http://localhost:5000"
echo "   Seq Logs: http://localhost:5341"