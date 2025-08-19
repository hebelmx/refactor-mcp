using ExxerFactor.Mcp.Web.Models;
using ExxerFactor.Mcp.Web.Services;
using IndexPage = ExxerFactor.Mcp.Web.Pages.Index;

namespace ExxerFactor.Mcp.Web.Tests.Components;

public class IndexPageTests : TestContext
{
    public IndexPageTests()
    {
        // Register MudBlazor services for testing
        // Services.AddMudServices(); // TODO: Add MudBlazor test services if needed

        // Register mock services
        var mockDashboardService = Substitute.For<IDashboardService>();
        var mockMetricsService = Substitute.For<IMetricsService>();

        mockDashboardService.GetDashboardStatsAsync().Returns(new DashboardStats(25, 3, 12, 2.1, 94));

        mockDashboardService.GetSystemHealthAsync().Returns(new SystemHealthStatus(true, "All systems operational", new Dictionary<string, ComponentHealth>
        {
            ["ExxerFactoringService"] = new ComponentHealth(true, "Healthy", null, DateTime.Now),
            ["McpServer"] = new ComponentHealth(true, "Healthy", null, DateTime.Now)
        }));

        mockDashboardService.GetRecentActivitiesAsync(Arg.Any<int>()).Returns(new[]
        {
            new ExxerFactoringActivity(DateTime.Now.AddMinutes(-5), "extract-method", "TestProject", true, TimeSpan.FromSeconds(2.3)),
            new ExxerFactoringActivity(DateTime.Now.AddMinutes(-10), "move-method", "AnotherProject", false, TimeSpan.FromSeconds(1.8), "Target class not found")
        });

        mockMetricsService.GetPerformanceMetricsAsync(Arg.Any<TimeSpan>()).Returns(new List<PerformanceMetric>
        {
            new PerformanceMetric(DateTime.Now, "Performance", 95.5, "%")
        });

        Services.AddSingleton(mockDashboardService);
        Services.AddSingleton(mockMetricsService);
    }

    [Fact]
    public void Index_ShouldRender_WithCorrectTitle()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Find("h3").TextContent.ShouldContain("Welcome to ExxerFactor.Mcp");
    }

    [Fact]
    public void Index_ShouldDisplay_StatsCards()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        // Should have cards for Available Tools, Total ExxerFactorings, Active Solutions, Success Rate
        var cards = component.FindAll(".mud-card");
        cards.Count.ShouldBeGreaterThanOrEqualTo(4);

        // Check for specific stats
        component.Markup.ShouldContain("Available Tools");
        component.Markup.ShouldContain("Total ExxerFactorings");
        component.Markup.ShouldContain("Active Solutions");
        component.Markup.ShouldContain("Success Rate");
    }

    [Fact]
    public void Index_ShouldDisplay_SystemHealthSection()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Markup.ShouldContain("System Health");
        component.Markup.ShouldContain("All systems operational");
        component.Markup.ShouldContain("ExxerFactoringService");
        component.Markup.ShouldContain("McpServer");
    }

    [Fact]
    public void Index_ShouldDisplay_RecentActivityTimeline()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Markup.ShouldContain("Recent Activity");
        component.Markup.ShouldContain("extract-method");
        component.Markup.ShouldContain("TestProject");
        component.Markup.ShouldContain("move-method");
        component.Markup.ShouldContain("AnotherProject");
    }

    [Fact]
    public void Index_ShouldDisplay_QuickActionButtons()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        component.Markup.ShouldContain("Quick Actions");
        component.Markup.ShouldContain("Browse Tools");
        component.Markup.ShouldContain("View Metrics");
        component.Markup.ShouldContain("System Monitor");
        component.Markup.ShouldContain("View Logs");

        // Check for correct links
        component.FindAll("a[href='/tools']").ShouldNotBeEmpty();
        component.FindAll("a[href='/metrics']").ShouldNotBeEmpty();
        component.FindAll("a[href='/monitoring']").ShouldNotBeEmpty();
        component.FindAll("a[href='/logs']").ShouldNotBeEmpty();
    }

    [Fact]
    public void Index_ShouldHave_RefreshButton()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        var refreshButton = component.FindAll(".mud-icon-button").FirstOrDefault();
        refreshButton.ShouldNotBeNull();
    }

    [Fact]
    public async Task Index_RefreshButton_ShouldTriggerDataRefresh()
    {
        // Arrange
        var component = RenderComponent<IndexPage>();
        var refreshButton = component.FindAll(".mud-icon-button").First();

        // Act
        await refreshButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        // The component should still render correctly after refresh
        component.Markup.ShouldContain("Welcome to ExxerFactor.Mcp");
        component.Markup.ShouldContain("System Health");
    }

    [Fact]
    public void Index_ShouldDisplay_StatsWithCorrectValues()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        // Check that the mock values are displayed
        component.Markup.ShouldContain("12"); // Available Tools
        component.Markup.ShouldContain("25"); // Total ExxerFactorings
        component.Markup.ShouldContain("3");  // Active Solutions
        component.Markup.ShouldContain("94"); // Success Rate
    }

    [Fact]
    public void Index_ShouldShowActivityDuration()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        // Should show completion times
        component.Markup.ShouldContain("2.3s");
        component.Markup.ShouldContain("1.8s");
    }

    [Fact]
    public void Index_ShouldDisplayHealthStatusCorrectly()
    {
        // Act
        var component = RenderComponent<IndexPage>();

        // Assert
        // Should show healthy status with success styling
        component.Markup.ShouldContain("Healthy");
        // MudAlert with Success severity should be present
        component.FindAll(".mud-alert-filled-success").ShouldNotBeEmpty();
    }
}

// Additional component tests can be added here for other pages