using ExxerFactor.Mcp.Web.Models;
using ExxerFactor.Mcp.Web.Services;

namespace ExxerFactor.Mcp.Web.Tests.Integration;

public class TestDashboardService : IDashboardService
{
    public Task<DashboardStats> GetDashboardStatsAsync()
    {
        return Task.FromResult(new DashboardStats(10, 2, 5, 1.5, 95));
    }

    public Task<IEnumerable<ExxerFactoringActivity>> GetRecentActivitiesAsync(int count = 20)
    {
        var activities = new[]
        {
            new ExxerFactoringActivity(DateTime.Now, "test-tool", "TestProject", true, TimeSpan.FromSeconds(1))
        };
        return Task.FromResult<IEnumerable<ExxerFactoringActivity>>(activities);
    }

    public Task<SystemHealthStatus> GetSystemHealthAsync()
    {
        var components = new Dictionary<string, ComponentHealth>
        {
            ["Test"] = new ComponentHealth(true, "Healthy", null, DateTime.Now)
        };
        return Task.FromResult(new SystemHealthStatus(true, "Healthy", components));
    }
}