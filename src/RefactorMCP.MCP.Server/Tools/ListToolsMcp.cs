using ModelContextProtocol.Server;

using System.ComponentModel;

namespace RefactorMCP.MCP.Server.Tools;

[McpServerToolType]
public class ListToolsMcp
{
    private readonly IRefactoringService _refactoringService;

    public ListToolsMcp(IRefactoringService refactoringService)
    {
        _refactoringService = refactoringService;
    }

    [McpServerTool, Description("List all available refactoring tools")]
    public async Task<string> ListToolsCommand()
    {
        var tools = await _refactoringService.ListAvailableToolsAsync();
        return string.Join('\n', tools.OrderBy(t => t));
    }
}