namespace RefactorMCP.UI.Models;

public record DashboardStats(
    int TotalRefactorings,
    int ActiveSolutions,
    int AvailableTools,
    double AverageExecutionTime,
    int SuccessRate
);

public record RefactoringActivity(
    DateTime Timestamp,
    string ToolName,
    string ProjectName,
    bool Success,
    TimeSpan Duration,
    string? ErrorMessage = null
);

public record SystemHealthStatus(
    bool IsHealthy,
    string Status,
    Dictionary<string, ComponentHealth> Components
);

public record ComponentHealth(
    bool IsHealthy,
    string Status,
    string? LastError = null,
    DateTime LastChecked = default
);

public record MetricsData(
    int RequestsPerMinute,
    double AverageResponseTime,
    int ActiveConnections,
    long MemoryUsage,
    double CpuUsage
);

public record PerformanceMetric(
    DateTime Timestamp,
    string MetricName,
    double Value,
    string Unit
);