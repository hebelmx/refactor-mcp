using RefactorMCP.UI.Models;

namespace RefactorMCP.UI.Services;

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<IEnumerable<RefactoringActivity>> GetRecentActivitiesAsync(int count = 20);
    Task<SystemHealthStatus> GetSystemHealthAsync();
}

public interface IMetricsService
{
    Task<MetricsData> GetMetricsDataAsync();
    Task<IEnumerable<PerformanceMetric>> GetPerformanceMetricsAsync(TimeSpan period);
}