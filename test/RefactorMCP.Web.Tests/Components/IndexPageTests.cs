using RefactorMCP.Web.Models;
using RefactorMCP.Web.Pages;
using RefactorMCP.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RefactorMCP.Web.Tests.Components;

public class IndexPageTests : TestContext
{
    public IndexPageTests()
    {
        // Register MudBlazor services for testing
        Services.AddMudServices();
        
        // Register mock services
        var mockDashboardService = new Mock<IDashboardService>();
        var mockMetricsService = new Mock<IMetricsService>();
        
        mockDashboardService.Setup(x => x.GetDashboardStatsAsync())
            .ReturnsAsync(new DashboardStats(25, 3, 12, 2.1, 94));
            
        mockDashboardService.Setup(x => x.GetSystemHealthAsync())
            .ReturnsAsync(new SystemHealthStatus(true, "All systems operational", new Dictionary<string, ComponentHealth>
            {
                ["RefactoringService"] = new ComponentHealth(true, "Healthy", DateTime.Now),
                ["McpServer"] = new ComponentHealth(true, "Healthy", DateTime.Now)
            }));
            
        mockDashboardService.Setup(x => x.GetRecentActivitiesAsync(It.IsAny<int>()))
            .ReturnsAsync(new[]
            {
                new RefactoringActivity(DateTime.Now.AddMinutes(-5), "extract-method", "TestProject", true, TimeSpan.FromSeconds(2.3)),
                new RefactoringActivity(DateTime.Now.AddMinutes(-10), "move-method", "AnotherProject", false, TimeSpan.FromSeconds(1.8), "Target class not found")
            });

        mockMetricsService.Setup(x => x.GetPerformanceMetricsAsync())
            .ReturnsAsync(new Dictionary<string, double> { ["performance"] = 95.5 });

        Services.AddSingleton(mockDashboardService.Object);
        Services.AddSingleton(mockMetricsService.Object);
    }

    [Fact]
    public void Index_ShouldRender_WithCorrectTitle()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        component.Find("h3").TextContent.Should().Contain("Welcome to RefactorMCP");
    }

    [Fact]
    public void Index_ShouldDisplay_StatsCards()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        // Should have cards for Available Tools, Total Refactorings, Active Solutions, Success Rate
        var cards = component.FindAll(".mud-card");
        cards.Count.Should().BeGreaterOrEqualTo(4);
        
        // Check for specific stats
        component.Markup.Should().Contain("Available Tools");
        component.Markup.Should().Contain("Total Refactorings");
        component.Markup.Should().Contain("Active Solutions");
        component.Markup.Should().Contain("Success Rate");
    }

    [Fact]
    public void Index_ShouldDisplay_SystemHealthSection()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        component.Markup.Should().Contain("System Health");
        component.Markup.Should().Contain("All systems operational");
        component.Markup.Should().Contain("RefactoringService");
        component.Markup.Should().Contain("McpServer");
    }

    [Fact]
    public void Index_ShouldDisplay_RecentActivityTimeline()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        component.Markup.Should().Contain("Recent Activity");
        component.Markup.Should().Contain("extract-method");
        component.Markup.Should().Contain("TestProject");
        component.Markup.Should().Contain("move-method");
        component.Markup.Should().Contain("AnotherProject");
    }

    [Fact]
    public void Index_ShouldDisplay_QuickActionButtons()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        component.Markup.Should().Contain("Quick Actions");
        component.Markup.Should().Contain("Browse Tools");
        component.Markup.Should().Contain("View Metrics");
        component.Markup.Should().Contain("System Monitor");
        component.Markup.Should().Contain("View Logs");
        
        // Check for correct links
        component.FindAll("a[href='/tools']").Should().NotBeEmpty();
        component.FindAll("a[href='/metrics']").Should().NotBeEmpty();
        component.FindAll("a[href='/monitoring']").Should().NotBeEmpty();
        component.FindAll("a[href='/logs']").Should().NotBeEmpty();
    }

    [Fact]
    public void Index_ShouldHave_RefreshButton()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        var refreshButton = component.FindAll(".mud-icon-button").FirstOrDefault();
        refreshButton.Should().NotBeNull();
    }

    [Fact]
    public async Task Index_RefreshButton_ShouldTriggerDataRefresh()
    {
        // Arrange
        var component = RenderComponent<Index>();
        var refreshButton = component.FindAll(".mud-icon-button").First();

        // Act
        await refreshButton.ClickAsync();

        // Assert
        // The component should still render correctly after refresh
        component.Markup.Should().Contain("Welcome to RefactorMCP");
        component.Markup.Should().Contain("System Health");
    }

    [Fact]
    public void Index_ShouldDisplay_StatsWithCorrectValues()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        // Check that the mock values are displayed
        component.Markup.Should().Contain("12"); // Available Tools
        component.Markup.Should().Contain("25"); // Total Refactorings  
        component.Markup.Should().Contain("3");  // Active Solutions
        component.Markup.Should().Contain("94"); // Success Rate
    }

    [Fact]
    public void Index_ShouldShowActivityDuration()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        // Should show completion times
        component.Markup.Should().Contain("2.3s");
        component.Markup.Should().Contain("1.8s");
    }

    [Fact]
    public void Index_ShouldDisplayHealthStatusCorrectly()
    {
        // Act
        var component = RenderComponent<Index>();

        // Assert
        // Should show healthy status with success styling
        component.Markup.Should().Contain("Healthy");
        // MudAlert with Success severity should be present
        component.FindAll(".mud-alert-filled-success").Should().NotBeEmpty();
    }
}

// Additional component tests can be added here for other pages
public class ToolsPageTests : TestContext
{
    // This would test the Tools.razor page once it's more developed
    // For now, just a placeholder structure
}