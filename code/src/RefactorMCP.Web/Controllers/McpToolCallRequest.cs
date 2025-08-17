using System.Text.Json;

namespace RefactorMCP.Web.Controllers;

public class McpToolCallRequest
{
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, JsonElement>? Parameters { get; set; }
}