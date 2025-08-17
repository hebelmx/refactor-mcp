namespace RefactorMCP.Web.Models;

public record ComponentHealth(
    bool IsHealthy,
    string Status,
    string? LastError = null,
    DateTime LastChecked = default
);