namespace RefactorMCP.Core.Abstractions;

public record RefactoringRequest(
    string ToolName,
    string SolutionPath,
    Dictionary<string, object> Parameters
);