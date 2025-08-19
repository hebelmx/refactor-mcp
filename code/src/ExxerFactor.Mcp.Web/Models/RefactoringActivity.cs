namespace ExxerFactor.Mcp.Web.Models;

public record ExxerFactoringActivity(
    DateTime Timestamp,
    string ToolName,
    string ProjectName,
    bool Success,
    TimeSpan Duration,
    string? ErrorMessage = null
);