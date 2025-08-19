using Microsoft.Extensions.Logging;
using ExxerFactor.Mcp.Web.Models;
using System.Diagnostics;

namespace ExxerFactor.Mcp.Web.Services;

public class MetricsService : IMetricsService
{
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }

    public async Task<MetricsData> GetMetricsDataAsync()
    {
        await Task.Delay(10);
        
        var process = Process.GetCurrentProcess();
        
        return new MetricsData(
            RequestsPerMinute: Random.Shared.Next(10, 100),
            AverageResponseTime: Random.Shared.NextDouble() * 500 + 100,
            ActiveConnections: Random.Shared.Next(1, 20),
            MemoryUsage: process.WorkingSet64,
            CpuUsage: Random.Shared.NextDouble() * 100
        );
    }

    public async Task<IEnumerable<PerformanceMetric>> GetPerformanceMetricsAsync(TimeSpan period)
    {
        await Task.Delay(10);
        
        var now = DateTime.UtcNow;
        var metrics = new List<PerformanceMetric>();
        
        for (int i = 0; i < 20; i++)
        {
            var timestamp = now.AddMinutes(-i * 5);
            metrics.Add(new PerformanceMetric(timestamp, "ResponseTime", Random.Shared.NextDouble() * 1000, "ms"));
            metrics.Add(new PerformanceMetric(timestamp, "RequestCount", Random.Shared.Next(5, 50), "req/min"));
            metrics.Add(new PerformanceMetric(timestamp, "MemoryUsage", Random.Shared.NextDouble() * 100, "MB"));
        }
        
        return metrics.OrderBy(m => m.Timestamp);
    }
}