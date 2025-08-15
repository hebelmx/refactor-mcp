#!/bin/bash

# Manual RefactorMCP Setup Commands
# Run these commands one by one in your terminal

echo "🚀 Manual RefactorMCP Setup"
echo "Run these commands one by one:"
echo ""

echo "1. Clean up Podman environment:"
echo "   unset DOCKER_HOST"
echo "   export DOCKER_HOST=''"
echo ""

echo "2. Test Docker connection:"
echo "   docker ps"
echo ""

echo "3. Create Seq container with persistent storage:"
echo "   sudo mkdir -p /opt/seq/data"
echo "   sudo chown 5341:5341 /opt/seq/data"
echo "   docker run -d --name refactor-mcp-seq --restart unless-stopped -p 5341:5341 -v /opt/seq/data:/data -e ACCEPT_EULA=Y datalust/seq:latest"
echo ""

echo "4. Build and publish RefactorMCP Web:"
echo "   dotnet build RefactorMCP.Web/RefactorMCP.Web.csproj --configuration Release"
echo "   dotnet publish RefactorMCP.Web/RefactorMCP.Web.csproj --configuration Release --output ./publish --no-build"
echo ""

echo "5. Install application:"
echo "   sudo mkdir -p /opt/refactor-mcp"
echo "   sudo cp -r ./publish/* /opt/refactor-mcp/"
echo "   sudo chown -R \$USER:\$USER /opt/refactor-mcp"
echo "   sudo chmod +x /opt/refactor-mcp/RefactorMCP.Web"
echo ""

echo "6. Create log directory:"
echo "   sudo mkdir -p /var/log/refactor-mcp"
echo "   sudo chown \$USER:\$USER /var/log/refactor-mcp"
echo ""

echo "7. Create production config:"
echo "   cat > /opt/refactor-mcp/appsettings.Production.json << 'CONFIG_EOF'"
echo '{'
echo '  "Logging": {'
echo '    "LogLevel": {'
echo '      "Default": "Information",'
echo '      "Microsoft.AspNetCore": "Warning"'
echo '    },'
echo '    "File": {'
echo '      "Path": "/var/log/refactor-mcp/refactor-mcp-.log"'
echo '    },'
echo '    "Seq": {'
echo '      "ServerUrl": "http://localhost:5341",'
echo '      "ApiKey": ""'
echo '    }'
echo '  },'
echo '  "AllowedHosts": "*"'
echo '}'
echo 'CONFIG_EOF'
echo ""

echo "8. Create systemd service:"
echo "   sudo tee /etc/systemd/system/refactor-mcp.service > /dev/null << 'SERVICE_EOF'"
echo '[Unit]'
echo 'Description=RefactorMCP Web Dashboard'
echo 'After=network.target docker.service'
echo 'Wants=docker.service'
echo ''
echo '[Service]'
echo 'Type=notify'
echo 'ExecStart=/usr/bin/dotnet /opt/refactor-mcp/RefactorMCP.Web.dll'
echo 'WorkingDirectory=/opt/refactor-mcp'
echo "User=\$USER"
echo "Group=\$USER"
echo 'Environment=ASPNETCORE_ENVIRONMENT=Production'
echo 'Environment=ASPNETCORE_URLS=http://0.0.0.0:5000;https://0.0.0.0:5001'
echo 'Restart=always'
echo 'RestartSec=10'
echo ''
echo '[Install]'
echo 'WantedBy=multi-user.target'
echo 'SERVICE_EOF'
echo ""

echo "9. Enable and start services:"
echo "   sudo systemctl daemon-reload"
echo "   sudo systemctl enable refactor-mcp.service"
echo "   sudo systemctl start refactor-mcp.service"
echo ""

echo "10. Check status:"
echo "    docker ps | grep seq"
echo "    sudo systemctl status refactor-mcp"
echo ""

echo "🌐 After setup, services will be available at:"
echo "   RefactorMCP Dashboard: http://localhost:5000"
echo "   Seq Logging: http://localhost:5341"
echo "   Metrics: http://localhost:5000/metrics"
echo ""