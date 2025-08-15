using System.Text.Json.Serialization;
using System.Text.Json;

namespace RefactorMCP.UI.Controllers;

[JsonSerializable(typeof(McpToolCallRequest))]
[JsonSerializable(typeof(McpToolCallResponse))]
[JsonSerializable(typeof(McpContent))]
[JsonSerializable(typeof(McpErrorResponse))]
[JsonSerializable(typeof(McpListToolsResponse))]
[JsonSerializable(typeof(McpTool))]
[JsonSerializable(typeof(McpServerInfo))]
[JsonSerializable(typeof(McpCapabilities))]
[JsonSerializable(typeof(McpToolsCapability))]
[JsonSerializable(typeof(McpResourcesCapability))]
[JsonSerializable(typeof(List<McpTool>))]
[JsonSerializable(typeof(List<McpContent>))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class McpJsonContext : JsonSerializerContext
{
}