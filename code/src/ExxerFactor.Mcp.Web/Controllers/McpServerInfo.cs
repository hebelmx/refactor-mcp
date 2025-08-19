namespace ExxerFactor.Mcp.Web.Controllers;

public class McpServerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ProtocolVersion { get; set; } = string.Empty;
    public McpCapabilities? Capabilities { get; set; }
}