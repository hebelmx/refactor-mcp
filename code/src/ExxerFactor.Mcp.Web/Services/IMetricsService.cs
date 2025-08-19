using ExxerFactor.Mcp.Web.Models;

namespace ExxerFactor.Mcp.Web.Services;

public interface IMetricsService
{
    Task<MetricsData> GetMetricsDataAsync();
    Task<IEnumerable<PerformanceMetric>> GetPerformanceMetricsAsync(TimeSpan period);
}