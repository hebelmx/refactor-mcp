using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RefactorMCP.Web.Models;
using RefactorMCP.Web.Services;

namespace RefactorMCP.Web.Tests;

/// <summary>
/// Base class for RefactorMCP.Web tests providing common web testing utilities
/// </summary>
public abstract class WebTestBase : IDisposable
{
    protected WebApplicationFactory<Program> Factory { get; }
    protected HttpClient Client { get; }

    protected WebTestBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    ConfigureTestServices(services);
                });
            });

        Client = Factory.CreateClient();
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Override with test implementations
        services.AddScoped<IDashboardService, MockDashboardService>();
        services.AddScoped<IMetricsService, MockMetricsService>();
    }

    public virtual void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Mock dashboard service for testing
/// </summary>
public class MockDashboardService : IDashboardService
{
    public Task<DashboardStats> GetDashboardStatsAsync()
    {
        return Task.FromResult(new DashboardStats(
            TotalRefactorings: 42,
            ActiveSolutions: 3,
            AvailableTools: 15,
            AverageExecutionTime: 1.8,
            SuccessRate: 94.5
        ));
    }

    public Task<IEnumerable<RefactoringActivity>> GetRecentActivitiesAsync(int count = 20)
    {
        var activities = new[]
        {
            new RefactoringActivity(
                Timestamp: DateTime.Now.AddMinutes(-2),
                ToolName: "extract-method",
                ProjectName: "WebApp.Core",
                Success: true,
                Duration: TimeSpan.FromSeconds(1.2)
            ),
            new RefactoringActivity(
                Timestamp: DateTime.Now.AddMinutes(-8),
                ToolName: "move-method",
                ProjectName: "DataAccess.Repository",
                Success: true,
                Duration: TimeSpan.FromSeconds(2.1)
            ),
            new RefactoringActivity(
                Timestamp: DateTime.Now.AddMinutes(-15),
                ToolName: "introduce-variable",
                ProjectName: "Business.Logic",
                Success: false,
                Duration: TimeSpan.FromSeconds(0.8),
                ErrorMessage: "Unable to resolve expression type"
            )
        };

        return Task.FromResult<IEnumerable<RefactoringActivity>>(activities.Take(count));
    }

    public Task<SystemHealthStatus> GetSystemHealthAsync()
    {
        var components = new Dictionary<string, ComponentHealth>
        {
            ["RefactoringService"] = new ComponentHealth(true, "Healthy", DateTime.Now.AddSeconds(-30)),
            ["McpServer"] = new ComponentHealth(true, "Healthy", DateTime.Now.AddSeconds(-45)),
            ["Database"] = new ComponentHealth(true, "Healthy", DateTime.Now.AddMinutes(-1)),
            ["Logging"] = new ComponentHealth(true, "Healthy", DateTime.Now.AddSeconds(-15))
        };

        return Task.FromResult(new SystemHealthStatus(
            IsHealthy: true,
            Status: "All systems operational",
            Components: components
        ));
    }
}

/// <summary>
/// Mock metrics service for testing
/// </summary>
public class MockMetricsService : IMetricsService
{
    public Task<string> GetMetricsJsonAsync()
    {
        var metrics = new
        {
            timestamp = DateTime.UtcNow,
            totalOperations = 156,
            successfulOperations = 148,
            failedOperations = 8,
            averageExecutionTime = 1.85,
            peakMemoryUsage = "245 MB",
            cpuUsage = 12.3,
            activeConnections = 4
        };

        return Task.FromResult(System.Text.Json.JsonSerializer.Serialize(metrics, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }

    public Task<Dictionary<string, double>> GetPerformanceMetricsAsync()
    {
        return Task.FromResult(new Dictionary<string, double>
        {
            ["cpu_usage"] = 12.3,
            ["memory_usage_mb"] = 245.7,
            ["disk_io_ops_per_sec"] = 15.2,
            ["network_throughput_mbps"] = 8.9,
            ["gc_collections_gen0"] = 23,
            ["gc_collections_gen1"] = 5,
            ["gc_collections_gen2"] = 1,
            ["thread_pool_threads"] = 8,
            ["working_set_mb"] = 189.4,
            ["response_time_ms"] = 1.85
        });
    }
}

/// <summary>
/// Test data builders for web components
/// </summary>
public static class WebTestDataBuilder
{
    public static DashboardStats CreateStats(
        int totalRefactorings = 100,
        int activeSolutions = 5,
        int availableTools = 12,
        double averageExecutionTime = 2.1,
        double successRate = 95.2)
    {
        return new DashboardStats(totalRefactorings, activeSolutions, availableTools, averageExecutionTime, successRate);
    }

    public static RefactoringActivity CreateActivity(
        string toolName = "test-tool",
        string projectName = "TestProject",
        bool success = true)
    {
        return new RefactoringActivity(
            DateTime.Now.AddMinutes(-Random.Shared.Next(1, 60)),
            toolName,
            projectName,
            success,
            TimeSpan.FromSeconds(Random.Shared.NextDouble() * 5 + 0.5),
            success ? null : "Test error message"
        );
    }

    public static SystemHealthStatus CreateHealthStatus(bool isHealthy = true)
    {
        var components = new Dictionary<string, ComponentHealth>
        {
            ["Service1"] = new ComponentHealth(isHealthy, isHealthy ? "Healthy" : "Unhealthy", DateTime.Now),
            ["Service2"] = new ComponentHealth(isHealthy, isHealthy ? "Healthy" : "Unhealthy", DateTime.Now),
        };

        return new SystemHealthStatus(isHealthy, isHealthy ? "All systems operational" : "System issues detected", components);
    }
}