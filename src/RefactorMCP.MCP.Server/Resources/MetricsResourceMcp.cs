using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;
using RefactorMCP.Core.Abstractions;
using System.ComponentModel;

namespace RefactorMCP.MCP.Server.Resources;

[McpServerResourceType]
public class MetricsResourceMcp
{
    private readonly IRefactoringService _refactoringService;

    public MetricsResourceMcp(IRefactoringService refactoringService)
    {
        _refactoringService = refactoringService;
    }

    [McpServerResource(UriTemplate = "metrics://{+path}")]
    [Description("Return code metrics for directories, files, classes or methods")]
    public async Task<TextResourceContents> ReadMetrics(
        [Description("Target path within the solution")] string path,
        [Description("Absolute path to the solution file (.sln)")] string solutionPath)
    {
        var metricsJson = await _refactoringService.GetMetricsAsync(solutionPath, path);
        return new TextResourceContents { Text = metricsJson };
    }
}