namespace RefactorMCP.Web.Models;

public record MetricsData(
    int RequestsPerMinute,
    double AverageResponseTime,
    int ActiveConnections,
    long MemoryUsage,
    double CpuUsage
);