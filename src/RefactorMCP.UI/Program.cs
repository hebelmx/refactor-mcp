using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using RefactorMCP.Core.Logging;
using RefactorMCP.MCP.Server.Extensions;
using RefactorMCP.UI.Components;
using RefactorMCP.UI.Components.Account;
using RefactorMCP.UI.Data;
using RefactorMCP.UI.Services;
using Serilog;

namespace RefactorMCP.UI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        LoggingConfiguration.ConfigureSerilog(builder.Configuration);
        builder.Host.UseSerilog();

        // Add MudBlazor services
        builder.Services.AddMudServices();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

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
                options.JsonSerializerOptions.TypeInfoResolverChain.Add(RefactorMCP.UI.Controllers.McpJsonContext.Default);
            });

        // Add RefactorMCP services
        builder.Services.AddRefactorMcpServer();

        // Add HTTP Client for Blazor components
        builder.Services.AddHttpClient();

        // Add custom services
        builder.Services.AddScoped<IDashboardService, DashboardService>();
        builder.Services.AddScoped<IMetricsService, MetricsService>();

        // Configure OpenTelemetry Metrics
        var serviceName = "RefactorMCP.UI";

        builder.Logging.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName))
                .AddConsoleExporter();
        });
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter());

        // Add health checks
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        // Add API controllers
        app.MapControllers();

        // Map OpenTelemetry metrics endpoint
        app.MapPrometheusScrapingEndpoint();

        // Map health checks
        app.MapHealthChecks("/health");

        // Welcome message
        Log.Information("RefactorMCP UI Server starting...");
        Log.Information("Dashboard available at: {Url}", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("Metrics endpoint: {Url}/metrics", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("Health check: {Url}/health", app.Urls.FirstOrDefault() ?? "https://localhost:5001");
        Log.Information("MCP HTTP API: {Url}/api/mcp", app.Urls.FirstOrDefault() ?? "https://localhost:5001");

        app.Run();
    }
}
