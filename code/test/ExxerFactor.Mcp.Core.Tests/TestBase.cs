namespace ExxerFactor.Mcp.Core.Tests;

/// <summary>
/// Base class for ExxerFactor.Mcp.Core tests providing common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected ServiceProvider ServiceProvider { get; }
    protected IServiceScope Scope { get; }

    protected TestBase()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        Scope = ServiceProvider.CreateScope();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(GetTestConfiguration())
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Add ExxerFactor.Mcp Core services
        services.AddExxerFactor.McpCore();
    }

    protected virtual Dictionary<string, string?> GetTestConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["Logging:LogLevel:Default"] = "Debug",
            ["ExxerFactor.Mcp:TestMode"] = "true"
        };
    }

    protected T GetService<T>() where T : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<T>();
    }

    protected T? GetOptionalService<T>()
    {
        return Scope.ServiceProvider.GetService<T>();
    }

    protected ILogger<T> GetLogger<T>()
    {
        return GetService<ILogger<T>>();
    }

    public virtual void Dispose()
    {
        Scope?.Dispose();
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}