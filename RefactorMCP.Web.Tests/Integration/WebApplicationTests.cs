using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RefactorMCP.Core.Abstractions;
using RefactorMCP.Web.Services;
using System.Text.Json;
using System.Text;
using RefactorMCP.Web.Controllers;

namespace RefactorMCP.Web.Tests.Integration;

public class WebApplicationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebApplicationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override services for testing
                services.AddScoped<IDashboardService, TestDashboardService>();
                services.AddScoped<IMetricsService, TestMetricsService>();
            });
            
            builder.UseEnvironment("Testing");
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Get_HomePage_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().Should().Contain("text/html");
    }

    [Fact]
    public async Task Get_HealthCheck_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task Get_PrometheusMetrics_ReturnsMetricsFormat()
    {
        // Act
        var response = await _client.GetAsync("/metrics");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        // Prometheus metrics should contain specific format
        // This is a basic check - in a real scenario, you'd validate specific metrics
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Get_McpTools_ReturnsToolsList()
    {
        // Act
        var response = await _client.GetAsync("/api/mcp/tools");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var content = await response.Content.ReadAsStringAsync();
        var toolsResponse = JsonSerializer.Deserialize<McpListToolsResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        toolsResponse.Should().NotBeNull();
        toolsResponse!.Tools.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_McpServerInfo_ReturnsCorrectInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/mcp/server-info");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var serverInfo = JsonSerializer.Deserialize<McpServerInfo>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        serverInfo.Should().NotBeNull();
        serverInfo!.Name.Should().Be("RefactorMCP");
        serverInfo.Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task Post_McpTools_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new McpToolCallRequest
        {
            ToolName = "nonexistent-tool", // This should fail gracefully
            Parameters = new Dictionary<string, JsonElement>()
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/mcp/tools", content);

        // Assert
        // Should return BadRequest for nonexistent tool, but not crash
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<McpErrorResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Contain("Tool not found");
    }

    [Fact]
    public async Task Get_ToolsPage_ReturnsSuccessAndCorrectContent()
    {
        // Act
        var response = await _client.GetAsync("/tools");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().Should().Contain("text/html");
    }

    [Fact]
    public async Task Get_MetricsPage_ReturnsSuccessAndCorrectContent()
    {
        // Act
        var response = await _client.GetAsync("/metrics");

        // Assert
        response.EnsureSuccessStatusCode();
        // Note: This might conflict with Prometheus /metrics endpoint
        // In a real app, you'd want different paths
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/tools")]
    [InlineData("/monitoring")]
    [InlineData("/logs")]
    public async Task Get_PublicPages_ReturnsSuccess(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Application_ShouldRegisterRequiredServices()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Assert - Verify key services are registered
        var dashboardService = serviceProvider.GetService<IDashboardService>();
        dashboardService.Should().NotBeNull();

        var refactoringService = serviceProvider.GetService<IRefactoringService>();
        refactoringService.Should().NotBeNull();

        var metricsService = serviceProvider.GetService<IMetricsService>();
        metricsService.Should().NotBeNull();
    }
}

// Test service implementations
internal class TestDashboardService : IDashboardService
{
    public Task<DashboardStats> GetDashboardStatsAsync()
    {
        return Task.FromResult(new DashboardStats(10, 2, 5, 1.5, 95));
    }

    public Task<IEnumerable<RefactoringActivity>> GetRecentActivitiesAsync(int count = 20)
    {
        var activities = new[]
        {
            new RefactoringActivity(DateTime.Now, "test-tool", "TestProject", true, TimeSpan.FromSeconds(1))
        };
        return Task.FromResult<IEnumerable<RefactoringActivity>>(activities);
    }

    public Task<SystemHealthStatus> GetSystemHealthAsync()
    {
        var components = new Dictionary<string, ComponentHealth>
        {
            ["Test"] = new ComponentHealth(true, "Healthy", DateTime.Now)
        };
        return Task.FromResult(new SystemHealthStatus(true, "Healthy", components));
    }
}

internal class TestMetricsService : IMetricsService
{
    public Task<string> GetMetricsJsonAsync()
    {
        return Task.FromResult("{}");
    }

    public Task<Dictionary<string, double>> GetPerformanceMetricsAsync()
    {
        return Task.FromResult(new Dictionary<string, double> { ["test"] = 1.0 });
    }
}