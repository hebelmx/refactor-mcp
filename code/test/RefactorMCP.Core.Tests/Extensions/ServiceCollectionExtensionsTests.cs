using Microsoft.Extensions.DependencyInjection;
using RefactorMCP.Core.Extensions;
using RefactorMCP.Core.Abstractions;

namespace RefactorMCP.Core.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRefactorMcpCore_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRefactorMcpCore();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify IRefactoringService is registered
        var refactoringService = serviceProvider.GetService<IRefactoringService>();
        refactoringService.Should().NotBeNull();

        // Verify it's registered as scoped
        var refactoringService1 = serviceProvider.GetService<IRefactoringService>();
        var refactoringService2 = serviceProvider.GetService<IRefactoringService>();

        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var scopedService1 = scope1.ServiceProvider.GetService<IRefactoringService>();
        var scopedService2 = scope2.ServiceProvider.GetService<IRefactoringService>();

        // Services should be different across scopes but same within scope
        scopedService1.Should().NotBeSameAs(scopedService2);
    }

    [Fact]
    public void AddRefactorMcpCore_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var act1 = () => services.AddRefactorMcpCore();
        var act2 = () => services.AddRefactorMcpCore();

        act1.Should().NotThrow();
        act2.Should().NotThrow();
    }

    [Fact]
    public void AddRefactorMcpCore_ShouldRegisterLoggingServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRefactorMcpCore();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should be able to resolve ILogger dependencies
        var refactoringService = serviceProvider.GetService<IRefactoringService>();
        refactoringService.Should().NotBeNull();
    }
}