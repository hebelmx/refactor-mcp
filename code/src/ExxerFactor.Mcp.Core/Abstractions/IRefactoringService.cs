using Microsoft.CodeAnalysis;

namespace ExxerFactor.Mcp.Core.Abstractions;

public interface IExxerFactoringService
{
    Task<ExxerFactoringResult> ExecuteExxerFactoringAsync(ExxerFactoringRequest request, CancellationToken cancellationToken = default);

    Task<ExxerFactoringResult> ExtractMethodAsync(string solutionPath, string filePath, int startLine, int endLine, string newMethodName, CancellationToken cancellationToken = default);

    Task<ExxerFactoringResult> MoveMethodAsync(string solutionPath, string sourceFilePath, string methodName, string targetClassName, CancellationToken cancellationToken = default);

    Task<ExxerFactoringResult> IntroduceVariableAsync(string solutionPath, string filePath, int line, int column, string variableName, CancellationToken cancellationToken = default);

    Task<string> GetMetricsAsync(string solutionPath, string path, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListAvailableToolsAsync();
}