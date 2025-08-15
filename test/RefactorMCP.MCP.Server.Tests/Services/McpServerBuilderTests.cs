using Microsoft.Extensions.DependencyInjection;
using RefactorMCP.MCP.Server.Services;
using RefactorMCP.Core.Abstractions;

namespace RefactorMCP.MCP.Server.Tests.Services;

// TODO: Rewrite these tests to match the actual McpServerBuilder API
// The current tests are testing a non-existent API
public class McpServerBuilderTests_DISABLED
{
    [Fact]
    public void Build_ShouldReturnNonNull_McpServerBuilder()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = McpServerBuilder.Create(services);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<McpServerBuilder>();
    }

    [Fact]
    public void WithRefactoringCore_ShouldRegisterCoreServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = McpServerBuilder.Create(services);

        // Act
        builder.WithRefactoringCore();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var refactoringService = serviceProvider.GetService<IRefactoringService>();
        refactoringService.Should().NotBeNull();
    }

    [Fact]
    public void WithStdioTransport_ShouldConfigureStdioTransport()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = McpServerBuilder.Create(services);

        // Act
        var result = builder.WithStdioTransport();

        // Assert
        result.Should().BeSameAs(builder); // Should return same instance for fluent interface
    }

    [Fact]
    public void WithHttpTransport_ShouldConfigureHttpTransport()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = McpServerBuilder.Create(services);

        // Act
        var result = builder.WithHttpTransport(7042);

        // Assert
        result.Should().BeSameAs(builder); // Should return same instance for fluent interface
    }

    [Fact]
    public void FluentInterface_ShouldAllowChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert - Should not throw
        var action = () => McpServerBuilder.Create(services)
            .WithRefactoringCore()
            .WithStdioTransport()
            .WithHttpTransport(7042);

        action.Should().NotThrow();
    }

    [Fact]
    public void Create_WithNullServices_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => McpServerBuilder.Create(null!);
        action.Should().Throw<ArgumentNullException>();
    }
}