using Microsoft.CodeAnalysis;

namespace RefactorMCP.Core.Abstractions;

public interface IRefactoringService
{
    Task<RefactoringResult> ExecuteRefactoringAsync(RefactoringRequest request, CancellationToken cancellationToken = default);

    Task<RefactoringResult> ExtractMethodAsync(string solutionPath, string filePath, int startLine, int endLine, string newMethodName, CancellationToken cancellationToken = default);

    Task<RefactoringResult> MoveMethodAsync(string solutionPath, string sourceFilePath, string methodName, string targetClassName, CancellationToken cancellationToken = default);

    Task<RefactoringResult> IntroduceVariableAsync(string solutionPath, string filePath, int line, int column, string variableName, CancellationToken cancellationToken = default);

    Task<string> GetMetricsAsync(string solutionPath, string path, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListAvailableToolsAsync();
}