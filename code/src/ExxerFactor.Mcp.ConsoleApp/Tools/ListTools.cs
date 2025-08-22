namespace ExxerFactor.Mcp.App.Tools;

[McpServerToolType]
public static class ListTools
{
    [McpServerTool, Description("List all available ExxerFactoring tools")]
    public static string ListToolsCommand()
    {
        var toolNames = typeof(LoadSolutionTool).Assembly
            .GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(McpServerToolTypeAttribute), false).Any())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any())
            .Select(m => ToKebabCase(m.Name))
            .OrderBy(n => n)
            .ToArray();

        return string.Join('\n', toolNames);
    }

    private static string ToKebabCase(string name)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0)
                sb.Append('-');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}