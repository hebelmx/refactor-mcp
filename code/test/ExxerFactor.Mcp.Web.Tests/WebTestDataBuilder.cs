using ExxerFactor.Mcp.Web.Models;

namespace ExxerFactor.Mcp.Web.Tests;

/// <summary>
/// Test data builders for web components
/// </summary>
public static class WebTestDataBuilder
{
    public static DashboardStats CreateStats(
        int totalExxerFactorings = 100,
        int activeSolutions = 5,
        int availableTools = 12,
        double averageExecutionTime = 2.1,
        int successRate = 95)
    {
        return new DashboardStats(totalExxerFactorings, activeSolutions, availableTools, averageExecutionTime, successRate);
    }

    public static ExxerFactoringActivity CreateActivity(
        string toolName = "test-tool",
        string projectName = "TestProject",
        bool success = true)
    {
        return new ExxerFactoringActivity(
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
            ["Service1"] = new ComponentHealth(isHealthy, isHealthy ? "Healthy" : "Unhealthy", null, DateTime.Now),
            ["Service2"] = new ComponentHealth(isHealthy, isHealthy ? "Healthy" : "Unhealthy", null, DateTime.Now),
        };

        return new SystemHealthStatus(isHealthy, isHealthy ? "All systems operational" : "System issues detected", components);
    }
}