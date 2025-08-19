using Microsoft.Extensions.DependencyInjection;
using ExxerFactor.Mcp.Core.Abstractions;
using ExxerFactor.Mcp.Core.Services;

namespace ExxerFactor.Mcp.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExxerFactorMcpCore(this IServiceCollection services)
    {
        services.AddSingleton<IExxerFactoringService, ExxerFactoringService>();

        // Add other core services here
        services.AddMemoryCache();

        return services;
    }
}