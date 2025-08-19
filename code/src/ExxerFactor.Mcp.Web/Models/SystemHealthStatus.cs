namespace ExxerFactor.Mcp.Web.Models;

public record SystemHealthStatus(
    bool IsHealthy,
    string Status,
    Dictionary<string, ComponentHealth> Components
);