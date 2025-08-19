namespace ExxerFactor.Mcp.Core.Abstractions;

public record ExxerFactoringResult(
    bool Success,
    string Message,
    string? UpdatedCode = null,
    IEnumerable<string>? ModifiedFiles = null,
    string? ErrorDetails = null
);