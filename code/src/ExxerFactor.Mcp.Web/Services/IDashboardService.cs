using ExxerFactor.Mcp.Web.Models;

namespace ExxerFactor.Mcp.Web.Services;

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync();

    Task<IEnumerable<ExxerFactoringActivity>> GetRecentActivitiesAsync(int count = 20);

    Task<SystemHealthStatus> GetSystemHealthAsync();
}