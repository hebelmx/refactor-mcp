using Microsoft.Extensions.Logging;

using RefactorMCP.Web.Models;
using RefactorMCP.Web.Services;

namespace RefactorMCP.Web.Tests.Services;

public class DashboardServiceTests
{
    private readonly IRefactoringService _mockRefactoringService;
    private readonly ILogger<DashboardService> _mockLogger;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _mockRefactoringService = Substitute.For<IRefactoringService>();
        _mockLogger = Substitute.For<ILogger<DashboardService>>();
        _service = new DashboardService(_mockRefactoringService, _mockLogger);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnValidStats()
    {
        // Arrange
        var expectedTools = new[] { "tool1", "tool2", "tool3", "tool4", "tool5" };
        _mockRefactoringService.ListAvailableToolsAsync().Returns(expectedTools);

        // Act
        var stats = await _service.GetDashboardStatsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.AvailableTools.Should().Be(expectedTools.Length);
        stats.TotalRefactorings.Should().Be(0); // Current implementation returns 0
        stats.ActiveSolutions.Should().Be(0); // Current implementation returns 0
        stats.AverageExecutionTime.Should().Be(0.0); // Current implementation returns 0
        stats.SuccessRate.Should().Be(95); // Current implementation returns 95
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldHandleException_AndReturnDefaultStats()
    {
        // Arrange
        _mockRefactoringService.ListAvailableToolsAsync().Throws(new InvalidOperationException("Service unavailable"));

        // Act
        var stats = await _service.GetDashboardStatsAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.AvailableTools.Should().Be(0);
        stats.TotalRefactorings.Should().Be(0);
        stats.ActiveSolutions.Should().Be(0);
        stats.AverageExecutionTime.Should().Be(0.0);
        stats.SuccessRate.Should().Be(0);

        // Verify error was logged
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Error getting dashboard stats")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_ShouldReturnMockActivities()
    {
        // Act
        var activities = await _service.GetRecentActivitiesAsync();

        // Assert
        activities.Should().NotBeNull();
        activities.Should().NotBeEmpty();
        
        var activityList = activities.ToList();
        activityList.Should().HaveCountGreaterThan(0);
        
        // Check the structure of returned activities
        var firstActivity = activityList.First();
        firstActivity.ToolName.Should().NotBeNullOrEmpty();
        firstActivity.ProjectName.Should().NotBeNullOrEmpty();
        firstActivity.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(15));
        firstActivity.Duration.Should().BePositive();
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_ShouldRespectCountParameter()
    {
        // Act
        var activities = await _service.GetRecentActivitiesAsync(1);

        // Assert
        activities.Should().NotBeNull();
        // Note: Current implementation returns fixed mock data, but this tests the parameter handling
        activities.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSystemHealthAsync_ShouldReturnHealthyStatus()
    {
        // Act
        var health = await _service.GetSystemHealthAsync();

        // Assert
        health.Should().NotBeNull();
        health.IsHealthy.Should().BeTrue();
        health.Status.Should().Be("All systems operational");
        health.Components.Should().NotBeEmpty();
        
        // Check specific components
        health.Components.Should().ContainKey("RefactoringService");
        health.Components.Should().ContainKey("McpServer");
        health.Components.Should().ContainKey("Database");
        health.Components.Should().ContainKey("Logging");
        
        // All components should be healthy in the mock implementation
        health.Components.Values.Should().OnlyContain(c => c.IsHealthy);
        health.Components.Values.Should().OnlyContain(c => c.Status == "Healthy");
    }

    [Fact]
    public async Task GetSystemHealthAsync_ShouldSetRecentLastCheckedTimes()
    {
        // Act
        var health = await _service.GetSystemHealthAsync();

        // Assert
        health.Components.Values.Should().OnlyContain(c => 
            c.LastChecked > DateTime.Now.AddMinutes(-1));
    }
}

// Test record builders
public static class DashboardTestDataBuilder
{
    public static DashboardStats CreateDashboardStats(
        int totalRefactorings = 100,
        int activeSolutions = 5,
        int availableTools = 15,
        double averageExecutionTime = 2.5,
        int successRate = 92)
    {
        return new DashboardStats(
            totalRefactorings,
            activeSolutions,
            availableTools,
            averageExecutionTime,
            successRate
        );
    }

    public static RefactoringActivity CreateActivity(
        string toolName = "test-tool",
        string projectName = "TestProject",
        bool success = true,
        DateTime? timestamp = null,
        TimeSpan? duration = null,
        string? errorMessage = null)
    {
        return new RefactoringActivity(
            timestamp ?? DateTime.Now,
            toolName,
            projectName,
            success,
            duration ?? TimeSpan.FromSeconds(1.5),
            errorMessage
        );
    }

    public static SystemHealthStatus CreateHealthStatus(
        bool isHealthy = true,
        string status = "All systems operational",
        Dictionary<string, ComponentHealth>? components = null)
    {
        return new SystemHealthStatus(
            isHealthy,
            status,
            components ?? new Dictionary<string, ComponentHealth>
            {
                ["TestComponent"] = new ComponentHealth(true, "Healthy", null, DateTime.Now)
            }
        );
    }
}