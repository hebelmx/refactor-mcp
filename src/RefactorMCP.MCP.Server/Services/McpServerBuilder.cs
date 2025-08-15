using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RefactorMCP.Core.Abstractions;

namespace RefactorMCP.MCP.Server.Services;

public class McpServerBuilder
{
    private readonly IServiceCollection _services;
    private readonly IHostBuilder _hostBuilder;

    public McpServerBuilder(IHostBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
        _services = new ServiceCollection();
    }

    public McpServerBuilder WithRefactoringTools()
    {
        _services
            .AddMcpServer()
            .WithToolsFromAssembly()
            .WithResourcesFromAssembly()
            .WithPromptsFromAssembly();

        return this;
    }

    public McpServerBuilder WithStdioTransport()
    {
        _services.AddMcpServer().WithStdioServerTransport();
        return this;
    }

    public McpServerBuilder WithWebSocketTransport(int port = 8080)
    {
        // TODO: Implement WebSocket transport for web integration
        return this;
    }

    public McpServerBuilder WithLogging(Action<ILoggingBuilder> configureLogging)
    {
        _hostBuilder.ConfigureLogging(configureLogging);
        return this;
    }

    public IHost Build()
    {
        _hostBuilder.ConfigureServices(services =>
        {
            foreach (var service in _services)
            {
                services.Add(service);
            }
        });

        return _hostBuilder.Build();
    }
}

public static class McpServerBuilderExtensions
{
    public static McpServerBuilder CreateMcpServerBuilder(this IHostBuilder hostBuilder)
    {
        return new McpServerBuilder(hostBuilder);
    }
}