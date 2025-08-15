# RefactorMCP Migration - Memory & Context

## 🎯 Mission Completed

Successfully migrated RefactorMCP console application to a web-based architecture with HTTP MCP server, proper logging, metrics, and observability.

## 📊 Project Status: ✅ COMPLETE

### ✅ Core Requirements Fulfilled
- [x] Convert console project to library structure
- [x] Add MudBlazor template with MCP functionality  
- [x] Implement logging, metrics and observability
- [x] Add welcome message with usage instructions
- [x] Expose HTTP MCP server alongside stdio MCP

## 🏗️ Architecture Implemented

### 3-Tier Structure
1. **RefactorMCP.Core** - Core library with 30+ C# refactoring tools
2. **RefactorMCP.MCP.Server** - MCP protocol implementation  
3. **RefactorMCP.Web** - MudBlazor web dashboard with HTTP MCP API

### Dual MCP Transport Support
- **Stdio MCP Server**: `./RefactorMCP.ConsoleApp` (for Claude Desktop)
- **HTTP MCP API**: `http://localhost:7042/api/mcp` (for web clients)

## 🌐 Current Services & Endpoints

### Primary Services
- **Web Dashboard**: `http://localhost:7042`
- **HTTPS Dashboard**: `https://localhost:7043`
- **Health Check**: `http://localhost:7042/health`
- **MCP HTTP API**: `http://localhost:7042/api/mcp/server-info`
- **Prometheus Metrics**: `http://localhost:7042/metrics`
- **Seq Logging**: `http://localhost:5341`

### Service Status
- ✅ **RefactorMCP Web Service**: Running on ports 7042/7043
- ✅ **Seq Logging Container**: Docker Compose managed
- ✅ **Systemd Integration**: Auto-start on boot with dependencies
- ✅ **JSON Serialization**: Fixed with source generation context

## 🛠️ Infrastructure Components

### Docker Compose Setup
```yaml
# /home/abel/projects/Refactor/refactor-mcp/docker-compose.yml
services:
  seq:
    image: datalust/seq:latest
    container_name: refactor-mcp-seq
    restart: unless-stopped
    ports: ["5341:5341"]
    volumes: ["/opt/seq/data:/data"]
```

### Systemd Services
- **refactor-mcp.service**: Main web application
- **refactor-mcp-seq.service**: Docker Compose for Seq

### Configuration Files
- **Service**: `/etc/systemd/system/refactor-mcp.service`
- **App Config**: `/opt/refactor-mcp/appsettings.Production.json`
- **Port Info**: `/opt/refactor-mcp/PORT_INFO.txt`

## 📁 5S Organized Script Structure

### Scripts Location: `./scripts/`
```
scripts/
├── refactor-mcp-manager.sh    # 🎯 Main entry point
├── setup/                     # 🚀 Installation scripts
├── maintenance/               # ⚙️ Updates and deployments
├── troubleshooting/           # 🔧 Debugging and fixes
├── testing/                   # 🧪 Validation scripts
├── common/                    # 📦 Shared utilities
└── archive/                   # 📚 Historical scripts
```

### Key Scripts
- **Manager**: `./scripts/refactor-mcp-manager.sh health`
- **Deploy**: `./scripts/maintenance/deploy-refactor-mcp.sh`
- **Docker**: `./scripts/maintenance/manage-docker-compose.sh`
- **Test API**: `./scripts/testing/test-mcp-api.sh`

## 🔧 Technical Solutions Implemented

### 1. Port Conflicts Resolution
- **Issue**: Port 5000 conflicts with common services
- **Solution**: Moved to ports 7042 (HTTP) and 7043 (HTTPS)

### 2. HTTPS Certificate Issues
- **Issue**: Production environment couldn't access dev certificates
- **Solution**: Mixed approach - Development environment with relaxed security

### 3. JSON Serialization in Trimmed Builds
- **Issue**: System.Text.Json metadata missing in self-contained builds
- **Solution**: Added JsonSerializerContext with source generation

### 4. Container Management
- **Issue**: Manual Docker commands caused conflicts
- **Solution**: Docker Compose with proper conflict resolution

### 5. Systemd Service Dependencies
- **Issue**: Services starting in wrong order
- **Solution**: 3-second delay and proper service dependencies

## 🧪 HTTP MCP API Usage

### Server Information
```bash
curl -s http://localhost:7042/api/mcp/server-info | jq .
```

### List Available Tools
```bash
curl -s http://localhost:7042/api/mcp/tools | jq '.tools[].name'
```

### Execute Tools
```bash
# Simple tool
curl -X POST http://localhost:7042/api/mcp/tools \
  -H 'Content-Type: application/json' \
  -d '{"toolName":"ListToolsCommand","parameters":{}}'

# Tool with parameters
curl -X POST http://localhost:7042/api/mcp/tools \
  -H 'Content-Type: application/json' \
  -d '{"toolName":"LoadSolution","parameters":{"solutionPath":"/path/to/solution.sln"}}'
```

## 🔍 Troubleshooting Quick Reference

### Common Issues & Solutions
1. **Service won't start**: `sudo journalctl -u refactor-mcp -f`
2. **Port conflicts**: `./scripts/troubleshooting/fix-port-conflict.sh`
3. **Container conflicts**: `./scripts/troubleshooting/fix-container-conflicts.sh`
4. **Docker environment**: `export DOCKER_HOST="unix:///var/run/docker.sock"`

### Health Checks
```bash
# Quick health check
./scripts/refactor-mcp-manager.sh health

# Service status
sudo systemctl status refactor-mcp

# Container status
docker compose ps
```

## 📈 Logging & Observability

### Structured Logging (Serilog)
- **Console**: Real-time logging
- **File**: `/var/log/refactor-mcp/refactor-mcp-*.log`  
- **Seq**: `http://localhost:5341` (structured logging server)

### Metrics (OpenTelemetry + Prometheus)
- **Endpoint**: `http://localhost:7042/metrics`
- **Includes**: HTTP requests, system resources, custom app metrics

### Monitoring
- **Health**: `http://localhost:7042/health`
- **Dashboard**: Built-in monitoring pages
- **Container**: Docker Compose health checks

## 🚀 Deployment Process

### Standard Deployment
```bash
./scripts/maintenance/deploy-refactor-mcp.sh
```

### Process Steps
1. Handle existing containers (clean removal)
2. Setup Seq services (Docker Compose)
3. Build application (self-contained .NET)
4. Deploy application (backup + install)
5. Restart services (proper sequence)
6. Test deployment (endpoint validation)
7. Cleanup (artifacts + old backups)

## 💡 Key Learnings & Best Practices

### Docker Environment
- Always check `DOCKER_HOST` environment variable
- Use `docker compose` (V2) instead of `docker-compose` (V1)
- Handle existing containers gracefully

### .NET Self-Contained Builds  
- Disable trimming for complex serialization scenarios
- Use JsonSerializerContext for AOT/trimming compatibility
- Consider build size vs. compatibility trade-offs

### Systemd Service Management
- Use `Type=simple` for standard web applications
- Avoid `Type=notify` unless app supports systemd notify protocol
- Add startup delays for service dependencies

### MCP Protocol Implementation
- Support both stdio and HTTP transports for maximum compatibility
- Provide clear API documentation and testing tools
- Use standard HTTP status codes and JSON responses

## 🎯 Next Steps (Future Enhancements)

### Potential Improvements
- [ ] WebSocket transport for real-time MCP communication
- [ ] Grafana dashboard for metrics visualization
- [ ] SSL certificate automation (Let's Encrypt)
- [ ] Load balancing for multiple instances
- [ ] Database persistence for operation history
- [ ] API rate limiting and authentication

### Maintenance Schedule
- **Weekly**: Run `./scripts/5s-maintenance.sh`
- **Monthly**: Update Docker images, clean old backups
- **Quarterly**: Review and update dependencies

## 📝 Files Created/Modified

### New Files
- `docker-compose.yml` - Seq service definition
- `scripts/refactor-mcp-manager.sh` - Main management interface
- `scripts/maintenance/deploy-refactor-mcp.sh` - Deployment automation
- `RefactorMCP.Web/Controllers/McpController.cs` - HTTP MCP API
- Multiple navigation pages (Tools, Logs, Monitoring, etc.)

### Modified Files  
- `RefactorMCP.Web/Program.cs` - Added HTTP MCP API, HttpClient service
- `/etc/systemd/system/refactor-mcp.service` - Fixed service configuration
- Various script organization and standardization

## 🔐 Security Considerations

### Implemented
- Systemd security settings (NoNewPrivileges, ProtectSystem, etc.)
- File permissions properly set
- Container isolation with dedicated network
- Non-root user execution

### Production Recommendations
- Enable HTTPS with proper certificates
- Add authentication to MCP API endpoints
- Implement rate limiting
- Regular security updates
- Log monitoring and alerting

---

**Migration Status**: ✅ **COMPLETE**  
**Last Updated**: $(date)  
**Next Checkpoint**: Server restart and full system validation