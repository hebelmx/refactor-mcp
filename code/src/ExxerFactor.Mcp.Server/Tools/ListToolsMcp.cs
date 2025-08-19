using System.ComponentModel;
using ModelContextProtocol.Server;
using ExxerFactor.Mcp.Core.Abstractions;

namespace ExxerFactor.Mcp.Server.Tools;

[McpServerToolType]
public class ListToolsMcp
{
    private readonly IExxerFactoringService _ExxerFactoringService;

    public ListToolsMcp(IExxerFactoringService ExxerFactoringService)
    {
        _ExxerFactoringService = ExxerFactoringService;
    }

    [McpServerTool, Description("List all available ExxerFactoring tools")]
    public async Task<string> ListToolsCommand()
    {
        var tools = await _ExxerFactoringService.ListAvailableToolsAsync();
        return string.Join('\n', tools.OrderBy(t => t));
    }
}