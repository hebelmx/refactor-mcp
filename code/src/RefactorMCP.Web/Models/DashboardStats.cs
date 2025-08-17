namespace RefactorMCP.Web.Models;

public record DashboardStats(
    int TotalRefactorings,
    int ActiveSolutions,
    int AvailableTools,
    double AverageExecutionTime,
    int SuccessRate
);