using System.Text.Json;

namespace ExxerFactor.Mcp.Web.Controllers;

public class McpToolCallRequest
{
    public string ToolName { get; set; } = string.Empty;
    public Dictionary<string, JsonElement>? Parameters { get; set; }
}