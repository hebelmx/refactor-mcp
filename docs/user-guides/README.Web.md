# RefactorMCP Web Dashboard

RefactorMCP has been migrated to a modern web-based architecture with comprehensive observability and monitoring capabilities.

## 🏗️ New Architecture

### Projects Structure:
- **RefactorMCP.Core** - Core refactoring library with all Roslyn-based tools
- **RefactorMCP.MCP.Server** - Model Context Protocol server implementation
- **RefactorMCP.Web** - MudBlazor web dashboard with real-time monitoring
- **RefactorMCP.ConsoleApp** - Legacy console application (maintained for compatibility)
- **RefactorMCP.Tests** - Comprehensive test suite

### Key Features:
✅ **MudBlazor Dashboard** - Modern web UI with real-time monitoring  
✅ **Serilog Logging** - Structured logging with File and Seq sinks  
✅ **OpenTelemetry Metrics** - Prometheus-compatible metrics endpoint  
✅ **Health Monitoring** - System health checks and component status  
✅ **MCP Integration** - Full Model Context Protocol server capabilities  
✅ **Real-time Analytics** - Performance metrics and usage statistics  

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Optional: Seq server for centralized logging (`docker run -d --name seq -p 5341:5341 -e ACCEPT_EULA=Y datalust/seq`)

### Running the Web Dashboard

```bash
# Start the web dashboard
cd RefactorMCP.Web
dotnet run

# Dashboard will be available at: https://localhost:5001
# Metrics endpoint: https://localhost:5001/metrics  
# Health check: https://localhost:5001/health
```

### Running the Legacy Console App

```bash
# Console MCP server (for compatibility)
cd RefactorMCP.ConsoleApp
dotnet run
```

## 📊 Observability Features

### Logging Configuration
Configure logging in `appsettings.json`:

```json
{
  "Logging": {
    "File": {
      "Path": "logs/refactor-mcp-.log"
    },
    "Seq": {
      "ServerUrl": "http://localhost:5341",
      "ApiKey": ""
    }
  }
}
```

### Metrics Endpoints
- **Dashboard**: `/` - Main dashboard with system overview
- **Metrics**: `/metrics` - Prometheus metrics endpoint
- **Health**: `/health` - Health check endpoint
- **Tools**: `/tools` - Available refactoring tools catalog
- **Monitoring**: `/monitoring` - Real-time system monitoring

### Available Sinks
- **Console**: Structured console output
- **File**: Rolling daily log files in `logs/` directory
- **Seq**: Centralized logging with rich search and analysis

## 🔧 Development

### Adding New Tools
1. Implement tool in `RefactorMCP.Core`
2. Create MCP wrapper in `RefactorMCP.MCP.Server`
3. Add UI components in `RefactorMCP.Web` if needed

### Dashboard Customization
- Modify pages in `RefactorMCP.Web/Pages/`
- Update components in `RefactorMCP.Web/Components/`
- Configure services in `RefactorMCP.Web/Services/`

## 📈 Monitoring Integration

The web dashboard provides comprehensive monitoring including:
- **Tool Usage Statistics** - Track refactoring operations
- **Performance Metrics** - Response times and throughput
- **System Health** - Component status and health checks
- **Real-time Logs** - Structured log viewer with filtering
- **MCP Connection Status** - Active client connections

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test RefactorMCP.Tests
```

## 📦 Deployment

### Docker Support (Coming Soon)
```bash
# Build container
docker build -t refactor-mcp-web .

# Run with observability stack
docker-compose up -d
```

### Production Configuration
- Configure Seq server URL in production settings
- Set up Prometheus for metrics collection
- Configure reverse proxy (nginx/traefik) for HTTPS
- Set up log retention policies

---

The web dashboard transforms RefactorMCP from a simple console tool into a comprehensive refactoring platform with enterprise-grade observability and monitoring capabilities.