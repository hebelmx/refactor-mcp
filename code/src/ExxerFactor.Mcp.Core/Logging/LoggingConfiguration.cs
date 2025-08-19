using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ExxerFactor.Mcp.Core.Logging;

public static class LoggingConfiguration
{
    public static void ConfigureSerilog(IConfiguration configuration)
    {
        var logConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ExxerFactor.Mcp")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File(
                path: configuration["Logging:File:Path"] ?? "logs/ExxerFactor-Mcp-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                shared: true
            );

        var seqUrl = configuration["Logging:Seq:ServerUrl"];
        if (!string.IsNullOrEmpty(seqUrl))
        {
            var seqApiKey = configuration["Logging:Seq:ApiKey"];
            logConfig.WriteTo.Seq(
                serverUrl: seqUrl,
                apiKey: seqApiKey,
                restrictedToMinimumLevel: LogEventLevel.Debug
            );
        }

        Log.Logger = logConfig.CreateLogger();
    }

    public static void AddSerilogToServices(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureSerilog(configuration);
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });
    }
}