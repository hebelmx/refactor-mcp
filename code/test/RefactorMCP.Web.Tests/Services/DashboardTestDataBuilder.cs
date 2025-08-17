using RefactorMCP.Web.Models;

namespace RefactorMCP.Web.Tests.Services;

public static class DashboardTestDataBuilder
{
    public static DashboardStats CreateDashboardStats(
        int totalRefactorings = 100,
        int activeSolutions = 5,
        int availableTools = 15,
        double averageExecutionTime = 2.5,
        int successRate = 92)
    {
        return new DashboardStats(
            totalRefactorings,
            activeSolutions,
            availableTools,
            averageExecutionTime,
            successRate
        );
    }

    public static RefactoringActivity CreateActivity(
        string toolName = "test-tool",
        string projectName = "TestProject",
        bool success = true,
        DateTime? timestamp = null,
        TimeSpan? duration = null,
        string? errorMessage = null)
    {
        return new RefactoringActivity(
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