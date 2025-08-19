using ExxerFactor.Mcp.Web.Models;
using ExxerFactor.Mcp.Web.Services;

namespace ExxerFactor.Mcp.Web.Tests;

/// <summary>
/// Mock dashboard service for testing
/// </summary>
public class MockDashboardService : IDashboardService
{
    public Task<DashboardStats> GetDashboardStatsAsync()
    {
        return Task.FromResult(new DashboardStats(
            TotalExxerFactorings: 42,
            ActiveSolutions: 3,
            AvailableTools: 15,
            AverageExecutionTime: 1.8,
            SuccessRate: 94
        ));
    }

    public Task<IEnumerable<ExxerFactoringActivity>> GetRecentActivitiesAsync(int count = 20)
    {
        var activities = new[]
        {
            new ExxerFactoringActivity(
                Timestamp: DateTime.Now.AddMinutes(-2),
                ToolName: "extract-method",
                ProjectName: "WebApp.Core",
                Success: true,
                Duration: TimeSpan.FromSeconds(1.2)
            ),
            new ExxerFactoringActivity(
                Timestamp: DateTime.Now.AddMinutes(-8),
                ToolName: "move-method",
                ProjectName: "DataAccess.Repository",
                Success: true,
                Duration: TimeSpan.FromSeconds(2.1)
            ),
            new ExxerFactoringActivity(
                Timestamp: DateTime.Now.AddMinutes(-15),
                ToolName: "introduce-variable",
                ProjectName: "Business.Logic",
                Success: false,
                Duration: TimeSpan.FromSeconds(0.8),
                ErrorMessage: "Unable to resolve expression type"
            )
        };

        return Task.FromResult<IEnumerable<ExxerFactoringActivity>>(activities.Take(count));
    }

    public Task<SystemHealthStatus> GetSystemHealthAsync()
    {
        var components = new Dictionary<string, ComponentHealth>
        {
            ["ExxerFactoringService"] = new ComponentHealth(true, "Healthy", null, DateTime.Now.AddSeconds(-30)),
            ["McpServer"] = new ComponentHealth(true, "Healthy", null, DateTime.Now.AddSeconds(-45)),
            ["Database"] = new ComponentHealth(true, "Healthy", null, DateTime.Now.AddMinutes(-1)),
            ["Logging"] = new ComponentHealth(true, "Healthy", null, DateTime.Now.AddSeconds(-15))
        };

        return Task.FromResult(new SystemHealthStatus(
            IsHealthy: true,
            Status: "All systems operational",
            Components: components
        ));
    }
}