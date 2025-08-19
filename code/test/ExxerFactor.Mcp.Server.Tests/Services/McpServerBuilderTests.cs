using ExxerFactor.Mcp.Server.Services;

namespace ExxerFactor.Mcp.Server.Tests.Services;

public class McpServerBuilderTests
{
    [Fact]
    public void Constructor_ShouldReturnNonNull_McpServerBuilder()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();

        // Act
        var builder = new McpServerBuilder(hostBuilder);

        // Assert
        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<McpServerBuilder>();
    }

    [Fact]
    public void CreateMcpServerBuilder_Extension_ShouldReturnBuilder()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();

        // Act
        var builder = hostBuilder.CreateMcpServerBuilder();

        // Assert
        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<McpServerBuilder>();
    }

    [Fact]
    public void WithStdioTransport_ShouldConfigureStdioTransport()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();
        var builder = new McpServerBuilder(hostBuilder);

        // Act
        var result = builder.WithStdioTransport();

        // Assert
        result.ShouldBeSameAs(builder); // Should return same instance for fluent interface
    }

    [Fact]
    public void WithWebSocketTransport_ShouldConfigureWebSocketTransport()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();
        var builder = new McpServerBuilder(hostBuilder);

        // Act
        var result = builder.WithWebSocketTransport(7042);

        // Assert
        result.ShouldBeSameAs(builder); // Should return same instance for fluent interface
    }

    [Fact]
    public void FluentInterface_ShouldAllowChaining()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();

        // Act & Assert - Should not throw
        var action = () => hostBuilder.CreateMcpServerBuilder()
            .WithExxerFactoringTools()
            .WithStdioTransport()
            .WithWebSocketTransport(7042);

        Should.NotThrow(action);
    }

    [Fact]
    public void Build_ShouldReturnIHost()
    {
        // Arrange
        var hostBuilder = Host.CreateDefaultBuilder();
        var builder = new McpServerBuilder(hostBuilder);

        // Act
        var host = builder.Build();

        // Assert
        host.ShouldNotBeNull();
        host.ShouldBeAssignableTo<IHost>();
    }

    [Fact]
    public void Constructor_WithNullHostBuilder_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new McpServerBuilder(null!);
        Should.Throw<ArgumentNullException>(action);
    }
}