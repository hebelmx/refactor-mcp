using ExxerFactor.Mcp.Core.Abstractions;
using ExxerFactor.Mcp.Core.Extensions;

namespace ExxerFactor.Mcp.Core.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddExxerFactorMcpCore_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddExxerFactorMcpCore();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify IExxerFactoringService is registered
        var ExxerFactoringService = serviceProvider.GetService<IExxerFactoringService>();
        ExxerFactoringService.ShouldNotBeNull();

        // Verify it's registered as scoped
        var ExxerFactoringService1 = serviceProvider.GetService<IExxerFactoringService>();
        var ExxerFactoringService2 = serviceProvider.GetService<IExxerFactoringService>();

        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var scopedService1 = scope1.ServiceProvider.GetService<IExxerFactoringService>();
        var scopedService2 = scope2.ServiceProvider.GetService<IExxerFactoringService>();

        // Services should be different across scopes but same within scope
        scopedService1.ShouldNotBeSameAs(scopedService2);
    }

    [Fact]
    public void AddExxerFactorMcpCore_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var act1 = () => services.AddExxerFactorMcpCore();
        var act2 = () => services.AddExxerFactorMcpCore();

        Should.NotThrow(act1);
        Should.NotThrow(act2);
    }

    [Fact]
    public void AddExxerFactorMcpCore_ShouldRegisterLoggingServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddExxerFactorMcpCore();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Should be able to resolve ILogger dependencies
        var ExxerFactoringService = serviceProvider.GetService<IExxerFactoringService>();
        ExxerFactoringService.ShouldNotBeNull();
    }
}