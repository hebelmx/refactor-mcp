using RefactorMCP.Web.Models;
using RefactorMCP.Web.Services;

namespace RefactorMCP.Web.Tests;

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
            SuccessRate: 94
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
            ["RefactoringService"] = new ComponentHealth(true, "Healthy", null, DateTime.Now.AddSeconds(-30)),
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