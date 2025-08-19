using System.Text.Json;
using ExxerFactor.Mcp.Core.Logging;
using ExxerFactor.Mcp.Web.Services;
using MudBlazor.Services;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using ExxerFactor.Mcp.Server.Extensions;
using Serilog;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        LoggingConfiguration.ConfigureSerilog(builder.Configuration);
        builder.Host.UseSerilog();

        // Add Blazor services
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        // Add MVC for API controllers with JSON serialization context
        builder.Services.AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                // Ensure all controller types are available
            })
            .AddJsonOptions(options =>
            {
                // Configure JSON serialization for trimming/AOT compatibility
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

                // Add the JSON context for trimming support
                options.JsonSerializerOptions.TypeInfoResolverChain.Add(ExxerFactor.Mcp.Web.Controllers.McpJsonContext.Default);
            });

        // Add MudBlazor
        builder.Services.AddMudServices();

        // Add ExxerFactor.Mcp services
        builder.Services.AddExxerFactorMcpServer();

        // Add HTTP Client for Blazor components
        builder.Services.AddHttpClient();

        // Add custom services
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IMetricsService, MetricsService>();

        // Configure OpenTelemetry Metrics

        var serviceName = "ExxerFactor.Mcp.Web";

        builder.Logging.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName));
        });
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter())
            .WithMetrics();
        // Add health checks
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapRazorPages();
        app.MapBlazorHub();
        app.MapControllers(); // Add API controllers
        app.MapFallbackToPage("/_Host");

        // Map OpenTelemetry metrics endpoint

        // Map health checks
        app.MapHealthChecks("/health");

        // Welcome message
        Log.Information("ExxerFactor.Mcp Web Server starting...");
        Log.Information("Dashboard available at: {Url}", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("Metrics endpoint: {Url}/metrics", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("Health check: {Url}/health", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("Mcp HTTP API: {Url}/api/Mcp", app.Urls.FirstOrDefault() ?? "https://localhost:5001");

        app.Run();
    }
}

namespace ExxerFactor.Mcp.Web
{
    public partial class Program
    { }
}