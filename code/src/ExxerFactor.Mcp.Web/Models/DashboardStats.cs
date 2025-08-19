namespace ExxerFactor.Mcp.Web.Models;

public record DashboardStats(
    int TotalExxerFactorings,
    int ActiveSolutions,
    int AvailableTools,
    double AverageExecutionTime,
    int SuccessRate
);