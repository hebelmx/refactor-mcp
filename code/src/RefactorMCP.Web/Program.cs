using System.Text.Json;
using RefactorMCP.Core.Logging;
using RefactorMCP.MCP.Server.Extensions;
using RefactorMCP.Web.Services;
using MudBlazor.Services;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

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
                options.JsonSerializerOptions.TypeInfoResolverChain.Add(RefactorMCP.Web.Controllers.McpJsonContext.Default);
            });

        // Add MudBlazor
        builder.Services.AddMudServices();

        // Add RefactorMCP services
        builder.Services.AddRefactorMcpServer();

        // Add HTTP Client for Blazor components
        builder.Services.AddHttpClient();

        // Add custom services
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IMetricsService, MetricsService>();

        // Configure OpenTelemetry Metrics

        var serviceName = "RefactorMCP.Web";

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
        app.MapPrometheusScrapingEndpoint();

        // Map health checks
        app.MapHealthChecks("/health");

        // Welcome message
        Log.Information("RefactorMCP Web Server starting...");
        Log.Information("Dashboard available at: {Url}", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("Metrics endpoint: {Url}/metrics", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("Health check: {Url}/health", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("MCP HTTP API: {Url}/api/mcp", app.Urls.FirstOrDefault() ?? "https://localhost:5001");

        app.Run();
    }
}

namespace RefactorMCP.Web
{
    public partial class Program
    { }
}