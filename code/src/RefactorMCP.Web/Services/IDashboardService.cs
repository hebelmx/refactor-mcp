using RefactorMCP.Web.Models;

namespace RefactorMCP.Web.Services;

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync();

    Task<IEnumerable<RefactoringActivity>> GetRecentActivitiesAsync(int count = 20);

    Task<SystemHealthStatus> GetSystemHealthAsync();
}