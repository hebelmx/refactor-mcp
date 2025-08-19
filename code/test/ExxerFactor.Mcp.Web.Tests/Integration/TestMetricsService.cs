using ExxerFactor.Mcp.Web.Models;
using ExxerFactor.Mcp.Web.Services;

namespace ExxerFactor.Mcp.Web.Tests.Integration;

public class TestMetricsService : IMetricsService
{
    public Task<MetricsData> GetMetricsDataAsync()
    {
        return Task.FromResult(new MetricsData(10, 1.5, 2, 1024 * 1024 * 100, 5.0));
    }

    public Task<IEnumerable<PerformanceMetric>> GetPerformanceMetricsAsync(TimeSpan period)
    {
        var metrics = new[] { new PerformanceMetric(DateTime.Now, "test", 1.0, "unit") };
        return Task.FromResult<IEnumerable<PerformanceMetric>>(metrics);
    }
}