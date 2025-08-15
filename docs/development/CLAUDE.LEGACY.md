# CLAUDE.md (Legacy Documentation)

This file contains the original CLAUDE.md guidance for working with the legacy console-based RefactorMCP architecture. See the main CLAUDE.md for current web-based architecture guidance.

## Common Commands (Legacy Console App)

### Build and Test
```bash
# Build the solution
dotnet build RefactorMCP.sln

# Run all tests
dotnet test

# Run a specific test file
dotnet test --filter "ClassName~MoveMethodsTests"

# Run the console application
dotnet run --project RefactorMCP.ConsoleApp

# Format code 
dotnet format
```

### Running RefactorMCP Tools (Legacy)
```bash
# JSON mode for tool invocation
dotnet run --project RefactorMCP.ConsoleApp -- --json <ToolName> '{"param":"value"}'

# Load solution (always run first)
dotnet run --project RefactorMCP.ConsoleApp -- --json load-solution '{"solutionPath":"./RefactorMCP.sln"}'

# List all available tools
dotnet run --project RefactorMCP.ConsoleApp -- --json ListTools '{}'
```

## Legacy Architecture Overview

The original RefactorMCP was a console application that provided C# refactoring capabilities through Roslyn. This has been migrated to a layered web architecture but the core principles remain:

### Core Components (Legacy)
- **Program.cs**: Entry point with MCP server hosting and JSON mode command execution
- **Tools/**: MCP server tools (refactoring operations) with `[McpServerTool]` attributes
- **SyntaxRewriters/**: Roslyn syntax transformations for code modification
- **SyntaxWalkers/**: AST traversal for analysis and information gathering
- **Move/**: Specialized move operation infrastructure

### Key Architectural Patterns (Legacy)

**Tool Pattern**: Each refactoring operation is implemented as a static method in a class marked with `[McpServerToolType]`. Methods are marked with `[McpServerTool]` and include detailed parameter descriptions.

**Caching Strategy**: Solutions are cached in `RefactoringHelpers.SolutionCache` to avoid reload overhead. Always call `LoadSolution` to reset caches between sessions.

**File Operations**: Uses `RefactoringHelpers.ReadFileWithEncodingAsync()` and `RefactoringHelpers.WriteFileWithEncodingAsync()` to preserve file encoding.

**AST Transformations**: 
- `SyntaxRewriters/` classes extend `CSharpSyntaxRewriter` to transform specific syntax nodes
- `SyntaxWalkers/` classes extend `CSharpSyntaxWalker` to collect information from AST
- Transformations preserve formatting using `Formatter.Format()`

**Move Operations**: Complex multi-step process in `Move/` directory:
- `MoveMethodAst.cs`: Core AST-based move logic
- `MoveMethodFileService.cs`: File-level coordination
- `MoveMethodHelpers.cs`: Utility functions
- History tracking prevents duplicate moves in same session

### Important Implementation Details (Legacy)

**Range Format**: Code selections use `"startLine:startColumn-endLine:endColumn"` (1-based indexing)

**Error Handling**: Throw `McpException` for user-facing errors. Include context and suggestions when possible.

**Dependency Ordering**: `MoveMultipleMethods.Tool.cs` uses `OrderOperations()` to move methods in dependency order.

**Session Management**: 
- `LoadSolution` clears all caches and resets move history
- Tool calls are logged to `.refactor-mcp/tool-call-log-*.jsonl`
- Metrics are cached in `.refactor-mcp/metrics/`

**Testing Structure**: 
- `RefactorMCP.Tests/` contains comprehensive test coverage
- Tests use `TestBase` class for common setup
- Separate test files for each tool and component
- Uses xUnit framework

### Code Organization Conventions (Legacy)

**Naming**: Tools use kebab-case externally (`extract-method`) but PascalCase internally (`ExtractMethod`)

**Parameter Injection**: Methods support both constructor injection (`this` parameter) and parameter injection for moved methods

**Wrapper Preservation**: Move operations leave delegating wrappers to maintain public API compatibility

**Static Analysis**: Extensive use of Roslyn semantic model for dependency analysis and type resolution

## Development Notes (Legacy)

- Always test refactorings on real C# code in the test suite
- Use `RefactoringHelpers.CreateWorkspace()` for MSBuild workspace creation
- Preserve existing code style and formatting when possible
- Handle edge cases like nested classes, generics, and overloaded methods
- Session logs can be replayed with `--cli play-log` for debugging