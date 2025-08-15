using Microsoft.Extensions.DependencyInjection;

using RefactorMCP.MCP.Server.Tools;
using RefactorMCP.MCP.Server.Resources;
using RefactorMCP.Core.Extensions;

namespace RefactorMCP.MCP.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRefactorMcpServer(this IServiceCollection services)
    {
        // Add core services
        services.AddRefactorMcpCore();

        // Add MCP tools and resources
        services.AddScoped<ListToolsMcp>();
        services.AddScoped<MetricsResourceMcp>();

        return services;
    }
}