using Microsoft.Extensions.Hosting;

namespace RefactorMCP.MCP.Server.Services;

public static class McpServerBuilderExtensions
{
    public static McpServerBuilder CreateMcpServerBuilder(this IHostBuilder hostBuilder)
    {
        return new McpServerBuilder(hostBuilder);
    }
}