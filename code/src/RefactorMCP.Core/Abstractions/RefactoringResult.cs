namespace RefactorMCP.Core.Abstractions;

public record RefactoringResult(
    bool Success,
    string Message,
    string? UpdatedCode = null,
    IEnumerable<string>? ModifiedFiles = null,
    string? ErrorDetails = null
);