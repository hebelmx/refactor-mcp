using ExxerFactor.Mcp.Web.Models;
using ExxerFactor.Mcp.Web.Services;

namespace ExxerFactor.Mcp.Web.Tests;

/// <summary>
/// Mock metrics service for testing
/// </summary>
public class MockMetricsService : IMetricsService
{
    public Task<MetricsData> GetMetricsDataAsync()
    {
        return Task.FromResult(new MetricsData(
            RequestsPerMinute: 156,
            AverageResponseTime: 1.85,
            ActiveConnections: 4,
            MemoryUsage: 245 * 1024 * 1024, // Convert MB to bytes
            CpuUsage: 12.3
        ));
    }

    public Task<IEnumerable<PerformanceMetric>> GetPerformanceMetricsAsync(TimeSpan period)
    {
        var now = DateTime.Now;
        var metrics = new[]
        {
            new PerformanceMetric(now.AddMinutes(-5), "CPU Usage", 12.3, "%"),
            new PerformanceMetric(now.AddMinutes(-5), "Memory Usage", 245.7, "MB"),
            new PerformanceMetric(now.AddMinutes(-5), "Disk IO", 15.2, "ops/sec"),
            new PerformanceMetric(now.AddMinutes(-5), "Network Throughput", 8.9, "Mbps")
        };

        return Task.FromResult<IEnumerable<PerformanceMetric>>(metrics);
    }
}