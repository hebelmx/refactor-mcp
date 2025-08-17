namespace RefactorMCP.Web.Models;

public record SystemHealthStatus(
    bool IsHealthy,
    string Status,
    Dictionary<string, ComponentHealth> Components
);