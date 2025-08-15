using Microsoft.Extensions.DependencyInjection;
using RefactorMCP.Core.Abstractions;
using RefactorMCP.Core.Services;

namespace RefactorMCP.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRefactorMcpCore(this IServiceCollection services)
    {
        services.AddSingleton<IRefactoringService, RefactoringService>();
        
        // Add other core services here
        services.AddMemoryCache();
        
        return services;
    }
}