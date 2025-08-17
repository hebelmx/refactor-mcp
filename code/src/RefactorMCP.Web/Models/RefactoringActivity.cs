namespace RefactorMCP.Web.Models;

public record RefactoringActivity(
    DateTime Timestamp,
    string ToolName,
    string ProjectName,
    bool Success,
    TimeSpan Duration,
    string? ErrorMessage = null
);