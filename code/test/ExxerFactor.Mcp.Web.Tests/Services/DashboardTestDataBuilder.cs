using ExxerFactor.Mcp.Web.Models;

namespace ExxerFactor.Mcp.Web.Tests.Services;

public static class DashboardTestDataBuilder
{
    public static DashboardStats CreateDashboardStats(
        int totalExxerFactorings = 100,
        int activeSolutions = 5,
        int availableTools = 15,
        double averageExecutionTime = 2.5,
        int successRate = 92)
    {
        return new DashboardStats(
            totalExxerFactorings,
            activeSolutions,
            availableTools,
            averageExecutionTime,
            successRate
        );
    }

    public static ExxerFactoringActivity CreateActivity(
        string toolName = "test-tool",
        string projectName = "TestProject",
        bool success = true,
        DateTime? timestamp = null,
        TimeSpan? duration = null,
        string? errorMessage = null)
    {
        return new ExxerFactoringActivity(
            timestamp ?? DateTime.Now,
            toolName,
            projectName,
            success,
            duration ?? TimeSpan.FromSeconds(1.5),
            errorMessage
        );
    }

    public static SystemHealthStatus CreateHealthStatus(
        bool isHealthy = true,
        string status = "All systems operational",
        Dictionary<string, ComponentHealth>? components = null)
    {
        return new SystemHealthStatus(
            isHealthy,
            status,
            components ?? new Dictionary<string, ComponentHealth>
            {
                ["TestComponent"] = new ComponentHealth(true, "Healthy", null, DateTime.Now)
            }
        );
    }
}