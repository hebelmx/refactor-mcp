namespace ExxerFactor.Mcp.Core.Abstractions;

public record ExxerFactoringRequest(
    string ToolName,
    string SolutionPath,
    Dictionary<string, object> Parameters
);