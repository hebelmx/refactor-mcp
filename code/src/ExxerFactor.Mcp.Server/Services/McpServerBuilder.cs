using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExxerFactor.Mcp.Server.Services;

public class McpServerBuilder
{
    private readonly IServiceCollection _services;
    private readonly IHostBuilder _hostBuilder;

    public McpServerBuilder(IHostBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder ?? throw new ArgumentNullException(nameof(hostBuilder));
        _services = new ServiceCollection();
    }

    public McpServerBuilder WithExxerFactoringTools()
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
        _hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddLogging(configureLogging);
        });
        return this;
    }

    public IHost Build()
    {
        _hostBuilder.ConfigureServices((context, services) =>
        {
            foreach (var service in _services)
            {
                services.Add(service);
            }
        });

        return _hostBuilder.Build();
    }
}