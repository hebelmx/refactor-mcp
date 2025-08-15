# Development Documentation

Resources, guides, and references for RefactorMCP contributors and developers.

## 👩‍💻 Developer Resources

### Getting Started
- **Development Setup** - Local development environment
- **Build and Test** - Building and running tests
- **Debugging Guide** - Debugging techniques and tools
- **IDE Configuration** - Recommended IDE setup

### Contributing
- **Contributing Guidelines** - How to contribute to the project
- **Code Standards** - Coding conventions and style guide
- **Pull Request Process** - PR submission and review
- **Issue Templates** - Bug reports and feature requests

### Code Examples
- [**Examples Collection**](EXAMPLES.md) - Comprehensive code examples and usage
- [**Test Suite Guide**](TEST_SUITE_README.md) - Understanding and running tests
- **Integration Examples** - How to integrate RefactorMCP
- **Custom Tool Development** - Creating new refactoring tools

### Legacy Documentation
- [**Agents Documentation**](AGENTS.md) - AI agent integration patterns
- [**Claude Legacy**](CLAUDE.LEGACY.md) - Legacy AI integration documentation  
- [**Claude Current**](CLAUDE.md) - Current AI integration setup
- [**MCP Testing**](test-mcp.md) - MCP protocol testing procedures

## 🛠️ Development Workflows

### Local Development
```bash
# Clone and setup
git clone <repository>
cd refactor-mcp

# Build all projects
dotnet build

# Run tests
dotnet test

# Start web dashboard
dotnet run --project src/RefactorMCP.Web
```

### Testing
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test test/RefactorMCP.Core.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Code Quality
```bash
# Format code
dotnet format

# Analyze code
dotnet analyze

# Security scan
dotnet list package --vulnerable
```

## 📚 Technical Guides

### Architecture
- **Component Design** - How components interact
- **Data Flow** - Understanding data movement
- **Extension Points** - Where to add new features
- **Performance Optimization** - Profiling and optimization

### Testing
- **Unit Testing** - Writing effective unit tests
- **Integration Testing** - End-to-end test scenarios  
- **Test Data Management** - Managing test fixtures
- **Mock Strategies** - Mocking external dependencies

### Deployment
- **Build Pipelines** - CI/CD configuration
- **Release Process** - How releases are made
- **Environment Management** - Dev/staging/prod setup
- **Monitoring** - Observability and logging

## 🐛 Troubleshooting

### Common Issues
- **Build Problems** - Compilation and dependency issues
- **Test Failures** - Common test failure patterns
- **Runtime Errors** - Common runtime issues
- **Performance Issues** - Identifying bottlenecks

### Debug Tools
- **Logging Configuration** - Setting up detailed logging
- **Profiling Tools** - Performance profiling setup
- **Memory Analysis** - Memory leak detection
- **Network Debugging** - HTTP and MCP debugging

## 📋 Checklists

### Pre-Commit Checklist
- [ ] Code builds without warnings
- [ ] All tests pass
- [ ] Code is formatted (`dotnet format`)
- [ ] No security vulnerabilities
- [ ] Documentation updated

### Release Checklist
- [ ] Version updated
- [ ] Changelog updated
- [ ] Tests passing on all platforms
- [ ] Documentation reviewed
- [ ] Security review completed

---

**Need help?** Check the troubleshooting section or ask in discussions.