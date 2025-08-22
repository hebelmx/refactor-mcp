namespace ExxerFactor.Mcp.App.Tools;

[McpServerToolType]
public static class VersionTool
{
    [McpServerTool, Description("Show the current version and build timestamp")]
    public static string Version()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "unknown";
        var buildTime = File.GetLastWriteTimeUtc(assembly.Location).ToString("u");
        return $"Version: {version} (Build {buildTime})";
    }
}