using ExxerFactor.Mcp.Web.Controllers;

namespace ExxerFactor.Mcp.Web.Tests.Controllers;

public static class McpTestDataBuilder
{
    public static McpToolCallRequest CreateToolCallRequest(string toolName, Dictionary<string, JsonElement>? parameters = null)
    {
        return new McpToolCallRequest
        {
            ToolName = toolName,
            Parameters = parameters ?? new Dictionary<string, JsonElement>()
        };
    }

    public static Dictionary<string, JsonElement> CreateParameters(params (string key, object value)[] parameters)
    {
        var result = new Dictionary<string, JsonElement>();
        foreach (var (key, value) in parameters)
        {
            var json = JsonSerializer.Serialize(value);
            result[key] = JsonSerializer.Deserialize<JsonElement>(json);
        }
        return result;
    }
}