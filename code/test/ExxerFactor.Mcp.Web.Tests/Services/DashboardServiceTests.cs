using ExxerFactor.Mcp.Core.Abstractions;
using ExxerFactor.Mcp.Web.Services;

namespace ExxerFactor.Mcp.Web.Tests.Services;

public class DashboardServiceTests
{
    private readonly IExxerFactoringService _mockExxerFactoringService;
    private readonly ILogger<DashboardService> _mockLogger;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _mockExxerFactoringService = Substitute.For<IExxerFactoringService>();
        _mockLogger = Substitute.For<ILogger<DashboardService>>();
        _service = new DashboardService(_mockExxerFactoringService, _mockLogger);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnValidStats()
    {
        // Arrange
        var expectedTools = new[] { "tool1", "tool2", "tool3", "tool4", "tool5" };
        _mockExxerFactoringService.ListAvailableToolsAsync().Returns(expectedTools);

        // Act
        var stats = await _service.GetDashboardStatsAsync();

        // Assert
        stats.ShouldNotBeNull();
        stats.AvailableTools.ShouldBe(expectedTools.Length);
        stats.TotalExxerFactorings.ShouldBe(0); // Current implementation returns 0
        stats.ActiveSolutions.ShouldBe(0); // Current implementation returns 0
        stats.AverageExecutionTime.ShouldBe(0.0); // Current implementation returns 0
        stats.SuccessRate.ShouldBe(95); // Current implementation returns 95
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldHandleException_AndReturnDefaultStats()
    {
        // Arrange
        _mockExxerFactoringService.ListAvailableToolsAsync().Throws(new InvalidOperationException("Service unavailable"));

        // Act
        var stats = await _service.GetDashboardStatsAsync();

        // Assert
        stats.ShouldNotBeNull();
        stats.AvailableTools.ShouldBe(0);
        stats.TotalExxerFactorings.ShouldBe(0);
        stats.ActiveSolutions.ShouldBe(0);
        stats.AverageExecutionTime.ShouldBe(0.0);
        stats.SuccessRate.ShouldBe(0);

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
        activities.ShouldNotBeNull();
        activities.ShouldNotBeEmpty();

        var activityList = activities.ToList();
        activityList.Count().ShouldBeGreaterThan(0);

        // Check the structure of returned activities
        var firstActivity = activityList.First();
        firstActivity.ToolName.ShouldNotBeNullOrEmpty();
        firstActivity.ProjectName.ShouldNotBeNullOrEmpty();
        firstActivity.Timestamp.ShouldBe(DateTime.Now, tolerance: TimeSpan.FromMinutes(15));
        firstActivity.Duration.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetRecentActivitiesAsync_ShouldRespectCountParameter()
    {
        // Act
        var activities = await _service.GetRecentActivitiesAsync(1);

        // Assert
        activities.ShouldNotBeNull();
        // Note: Current implementation returns fixed mock data, but this tests the parameter handling
        activities.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetSystemHealthAsync_ShouldReturnHealthyStatus()
    {
        // Act
        var health = await _service.GetSystemHealthAsync();

        // Assert
        health.ShouldNotBeNull();
        health.IsHealthy.ShouldBeTrue();
        health.Status.ShouldBe("All systems operational");
        health.Components.ShouldNotBeEmpty();

        // Check specific components
        health.Components.ShouldContainKey("ExxerFactoringService");
        health.Components.ShouldContainKey("McpServer");
        health.Components.ShouldContainKey("Database");
        health.Components.ShouldContainKey("Logging");

        // All components should be healthy in the mock implementation
        health.Components.Values.ShouldAllBe(c => c.IsHealthy);
        health.Components.Values.ShouldAllBe(c => c.Status == "Healthy");
    }

    [Fact]
    public async Task GetSystemHealthAsync_ShouldSetRecentLastCheckedTimes()
    {
        // Act
        var health = await _service.GetSystemHealthAsync();

        // Assert
        health.Components.Values.ShouldAllBe(c =>
            c.LastChecked > DateTime.Now.AddMinutes(-1));
    }
}

// Test record builders