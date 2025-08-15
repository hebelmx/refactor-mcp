using System.ComponentModel;
using ModelContextProtocol.Server;

namespace RefactorMCP.Core.Tools;

[McpServerToolType]
public static class UnloadSolutionTool
{
    [McpServerTool, Description("Unload a solution and remove it from the cache")]
    public static string UnloadSolution(
        [Description("Absolute path to the solution file (.sln)")] string solutionPath,
        CancellationToken cancellationToken = default)
    {
        if (RefactoringHelpers.SolutionCache.TryGetValue(solutionPath, out _))
        {
            RefactoringHelpers.SolutionCache.Remove(solutionPath);
            return $"Unloaded solution '{Path.GetFileName(solutionPath)}' from cache";
        }

        return $"Solution '{Path.GetFileName(solutionPath)}' was not loaded";
    }

    [McpServerTool, Description("Clear all cached solutions")]
    public static string ClearSolutionCache(
        CancellationToken cancellationToken = default)
    {
        RefactoringHelpers.ClearAllCaches();
        return "Cleared all cached solutions";
    }
}