# RefactorMCP

**RefactorMCP** is a comprehensive Model Context Protocol (MCP) server that exposes powerful Roslyn-based refactoring tools for C# projects. It provides both console and web interfaces for advanced code refactoring operations.

## 🏗️ Architecture

RefactorMCP follows a clean, modular architecture:

```
📦 RefactorMCP
├── 📁 src/                     # Source code
│   ├── RefactorMCP.Core/       # Core refactoring logic
│   ├── RefactorMCP.MCP.Server/ # MCP protocol implementation
│   └── RefactorMCP.Web/        # Blazor web interface
├── 📁 test/                    # Test projects
├── 📁 docs/                    # Documentation
├── 📁 scripts/                 # Automation scripts
├── 📁 config/                  # Configuration files
└── 📁 tools/                   # Additional tools
```

## 🚀 Quick Start

### Console Application
```bash
dotnet run --project src/RefactorMCP.ConsoleApp
```

### Web Dashboard
```bash
dotnet run --project src/RefactorMCP.Web
```

### MCP Server Integration
```bash
# Host as MCP server (see docs/user-guides/ for details)
dotnet build src/RefactorMCP.MCP.Server
```

## 🛠️ Available Refactoring Tools

### **Method Operations**
- **Extract Method** – Create new methods from code blocks
- **Inline Method** – Replace method calls with method body
- **Move Methods** – Relocate methods between classes
- **Move Multiple Methods** – Batch move operations with dependency injection

### **Class & Type Operations**
- **Extract Class** – Create new classes from existing members
- **Move Type to File** – Separate types into dedicated files
- **Extract Decorator/Adapter** – Generate design pattern implementations
- **Use Interface** – Convert concrete types to interface usage

### **Field & Property Operations**
- **Introduce Field/Parameter/Variable** – Create new class members
- **Make Field Readonly** – Convert fields to readonly with constructor init
- **Transform Setter to Init** – Convert properties to init-only
- **Constructor Injection** – Convert parameters to injected dependencies

### **Code Quality**
- **Safe Delete** – Remove unused code with dependency validation
- **Convert to Static** – Make methods static with proper parameter handling
- **Add Observer** – Introduce event-driven patterns
- **Cleanup Usings** – Organize import statements

### **Analysis & Metrics**
- **Analyze Refactoring Opportunities** – Identify improvement areas
- **Class Length Metrics** – Measure class complexity
- **List Available Tools** – Show all refactoring options

## 📚 Documentation

| Category | Location | Description |
|----------|----------|-------------|
| **User Guides** | [`docs/user-guides/`](docs/user-guides/) | Step-by-step usage instructions |
| **Architecture** | [`docs/architecture/`](docs/architecture/) | System design and technical details |
| **Development** | [`docs/development/`](docs/development/) | Contributor guides and examples |
| **API Reference** | [`docs/api/`](docs/api/) | API documentation |
| **Deployment** | [`docs/deployment/`](docs/deployment/) | Deployment guides |

## 🔧 Development

### Building
```bash
# Build all projects
dotnet build

# Run tests
dotnet test

# Format code
dotnet format
```

### Project Structure
- **RefactorMCP.Core** - Core refactoring engine
- **RefactorMCP.MCP.Server** - MCP protocol implementation  
- **RefactorMCP.Web** - Blazor web interface with API
- **RefactorMCP.ConsoleApp** - Command-line interface

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/RefactorMCP.Core.Tests
```

## 📁 Scripts & Tools

| Script | Purpose |
|--------|---------|
| [`scripts/setup/`](scripts/setup/) | Environment setup scripts |
| [`scripts/maintenance/`](scripts/maintenance/) | Maintenance and deployment |
| [`scripts/testing/`](scripts/testing/) | Testing utilities |
| [`tools/vscode-extension/`](tools/vscode-extension/) | VS Code extension |

## 🌐 Web Interface Features

- **Interactive Dashboard** - Real-time refactoring operations
- **Metrics & Analytics** - Performance and usage tracking  
- **Tool Management** - Browse and execute refactoring tools
- **Project Monitoring** - Track refactoring activities
- **HTTP API** - RESTful endpoints for automation

## 🔗 Integration

### Model Context Protocol (MCP)
RefactorMCP implements the MCP standard for seamless integration with AI development tools.

### Supported Formats
- JSON-RPC over stdio
- HTTP endpoints  
- WebSocket connections

## 📄 License

Licensed under the [Mozilla Public License 2.0](https://www.mozilla.org/MPL/2.0/).

## 🤝 Contributing

1. Read [`docs/development/`](docs/development/) for contributor guidelines
2. Run tests: `dotnet test`
3. Format code: `dotnet format` 
4. Submit pull requests with clear descriptions

---

For detailed examples and advanced usage, see the documentation in [`docs/`](docs/).