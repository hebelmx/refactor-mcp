using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ExxerFactor.Mcp.Core.Tools;

[McpServerToolType]
public static class UnloadSolutionTool
{
    [McpServerTool, Description("Unload a solution and remove it from the cache")]
    public static string UnloadSolution(
        [Description("Absolute path to the solution file (.sln)")] string solutionPath,
        CancellationToken cancellationToken = default)
    {
        if (ExxerFactoringHelpers.SolutionCache.TryGetValue(solutionPath, out _))
        {
            ExxerFactoringHelpers.SolutionCache.Remove(solutionPath);
            return $"Unloaded solution '{Path.GetFileName(solutionPath)}' from cache";
        }

        return $"Solution '{Path.GetFileName(solutionPath)}' was not loaded";
    }

    [McpServerTool, Description("Clear all cached solutions")]
    public static string ClearSolutionCache(
        CancellationToken cancellationToken = default)
    {
        ExxerFactoringHelpers.ClearAllCaches();
        return "Cleared all cached solutions";
    }
}