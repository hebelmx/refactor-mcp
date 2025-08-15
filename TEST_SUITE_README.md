# RefactorMCP Test Suite Documentation

## Overview

This comprehensive test suite covers all layers of the RefactorMCP architecture with unit tests, integration tests, and component tests.

## Test Projects Structure

```
RefactorMCP.Core.Tests/           # Core library unit tests
├── Services/
│   └── RefactoringServiceTests.cs
├── Extensions/
│   └── ServiceCollectionExtensionsTests.cs
└── TestBase.cs                   # Common test utilities

RefactorMCP.MCP.Server.Tests/     # MCP server layer tests
└── Services/
    └── McpServerBuilderTests.cs

RefactorMCP.Web.Tests/            # Web application tests
├── Controllers/
│   └── McpControllerTests.cs
├── Services/
│   └── DashboardServiceTests.cs
├── Integration/
│   └── WebApplicationTests.cs
├── Components/
│   └── IndexPageTests.cs
└── TestBase.cs                   # Web test utilities
```

## Test Categories

### Unit Tests
- **RefactoringService**: Core business logic and service methods
- **DashboardService**: Dashboard data aggregation and health monitoring
- **McpController**: HTTP API endpoint logic
- **ServiceCollectionExtensions**: Dependency injection configuration

### Integration Tests
- **WebApplicationTests**: Full HTTP pipeline testing
- **McpServerBuilderTests**: MCP server configuration and setup

### Component Tests
- **IndexPageTests**: Blazor component rendering and interaction using bUnit
- **Navigation and UI Tests**: User interface behavior

## Testing Technologies

- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies  
- **FluentAssertions**: Improved assertion syntax
- **bUnit**: Blazor component testing
- **Microsoft.AspNetCore.Mvc.Testing**: Integration testing for web applications
- **coverlet**: Code coverage analysis

## Running Tests

### All Tests
```bash
# Run all tests in solution
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Specific Test Projects
```bash
# Core library tests
dotnet test RefactorMCP.Core.Tests

# MCP server tests  
dotnet test RefactorMCP.MCP.Server.Tests

# Web application tests
dotnet test RefactorMCP.Web.Tests
```

### Test Categories
```bash
# Run only fast unit tests
dotnet test --filter "Category!=Integration"

# Run integration tests
dotnet test --filter "Category=Integration"
```

## Test Data and Utilities

### TestBase Classes
- **RefactorMCP.Core.Tests.TestBase**: Provides service provider setup for core tests
- **RefactorMCP.Web.Tests.WebTestBase**: Provides web application factory for web tests

### Test Data Builders
- **McpTestDataBuilder**: Creates MCP request/response objects
- **DashboardTestDataBuilder**: Creates dashboard model objects  
- **WebTestDataBuilder**: Creates web-specific test data

### Mock Services
- **MockDashboardService**: Provides realistic test data for dashboard
- **MockMetricsService**: Provides performance metrics test data

## Test Patterns

### Arrange-Act-Assert Pattern
```csharp
[Fact]
public async Task GetDashboardStatsAsync_ShouldReturnValidStats()
{
    // Arrange
    var expectedTools = new[] { "tool1", "tool2", "tool3" };
    _mockRefactoringService.Setup(x => x.ListAvailableToolsAsync())
        .ReturnsAsync(expectedTools);

    // Act
    var stats = await _service.GetDashboardStatsAsync();

    // Assert
    stats.Should().NotBeNull();
    stats.AvailableTools.Should().Be(expectedTools.Length);
}
```

### Integration Test Pattern
```csharp
[Fact]
public async Task Get_HomePage_ReturnsSuccessAndCorrectContentType()
{
    // Act
    var response = await _client.GetAsync("/");

    // Assert
    response.EnsureSuccessStatusCode();
    response.Content.Headers.ContentType?.ToString().Should().Contain("text/html");
}
```

### Component Test Pattern
```csharp
[Fact]
public void Index_ShouldDisplay_StatsCards()
{
    // Act
    var component = RenderComponent<Index>();

    // Assert
    var cards = component.FindAll(".mud-card");
    cards.Count.Should().BeGreaterOrEqualTo(4);
    component.Markup.Should().Contain("Available Tools");
}
```

## Mock Strategy

### Service Layer Mocking
- Mock external dependencies (IRefactoringService, ILogger)
- Use realistic return values that match production scenarios
- Test both success and failure scenarios

### Web Layer Testing
- Use TestHost for integration tests
- Override services with test implementations
- Test full HTTP request/response cycle

### Component Testing
- Mock service dependencies at component level
- Test rendering, user interactions, and state changes
- Verify correct DOM structure and content

## Coverage Goals

- **Unit Tests**: > 90% line coverage for core business logic
- **Integration Tests**: Cover all HTTP endpoints and major user flows
- **Component Tests**: Cover all user-facing components and interactions

## Continuous Integration

Tests are designed to run reliably in CI/CD environments:
- No external dependencies (databases, web services)
- Deterministic test data and timing
- Proper cleanup and isolation between tests
- Cross-platform compatibility (.NET 9.0)

## Development Guidelines

### Writing New Tests
1. Follow AAA pattern (Arrange-Act-Assert)
2. Use descriptive test names that explain the scenario
3. Create focused tests that test one thing
4. Use appropriate builders for test data creation
5. Mock dependencies appropriately

### Test Maintenance  
1. Keep tests up-to-date with code changes
2. Refactor tests when functionality changes
3. Remove obsolete tests for removed features
4. Update test data builders for new model properties

### Performance Considerations
- Use `Task.FromResult()` for async mocks when possible
- Minimize setup overhead in test constructors
- Use test categories to separate fast/slow tests
- Consider parallel test execution for large suites

## Future Enhancements

- **Load Testing**: Add performance tests for high-volume scenarios
- **End-to-End Tests**: Browser automation for complete user workflows  
- **Contract Testing**: API contract validation for MCP protocol compliance
- **Property-Based Testing**: Generate test cases for edge scenarios
- **Mutation Testing**: Validate test suite effectiveness