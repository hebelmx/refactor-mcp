using Microsoft.Extensions.DependencyInjection;
using ExxerFactor.Mcp.Server.Resources;
using ExxerFactor.Mcp.Server.Tools;
using ExxerFactor.Mcp.Core.Extensions;

namespace ExxerFactor.Mcp.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExxerFactorMcpServer(this IServiceCollection services)
    {
        // Add core services
        services.AddExxerFactorMcpCore();

        // Add Mcp tools and resources
        services.AddScoped<ListToolsMcp>();
        services.AddScoped<MetricsResourceMcp>();

        return services;
    }
}