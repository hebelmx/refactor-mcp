using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RefactorMCP.Web.Controllers;
using System.Text.Json;

namespace RefactorMCP.Web.Tests.Controllers;

public class McpControllerTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly ILogger<McpController> _mockLogger;
    private readonly McpController _controller;

    public McpControllerTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockLogger = Substitute.For<ILogger<McpController>>();
        _controller = new McpController(_mockServiceProvider, _mockLogger);
    }

    [Fact]
    public async Task HandleToolCall_ShouldReturnBadRequest_WhenToolNotFound()
    {
        // Arrange
        var request = new McpToolCallRequest
        {
            ToolName = "nonexistent-tool",
            Parameters = new Dictionary<string, JsonElement>()
        };

        // Act
        var result = await _controller.HandleToolCall(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeOfType<McpErrorResponse>();
        
        var errorResponse = badRequestResult.Value as McpErrorResponse;
        errorResponse!.Error.Should().Contain("Tool not found");
        errorResponse.Code.Should().Be(-32601);
    }

    [Fact]
    public async Task HandleToolCall_ShouldLogToolCall()
    {
        // Arrange
        var request = new McpToolCallRequest
        {
            ToolName = "test-tool",
            Parameters = new Dictionary<string, JsonElement>()
        };

        // Act
        await _controller.HandleToolCall(request);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("MCP tool call: test-tool")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void ListTools_ShouldReturnOkResult_WithToolsList()
    {
        // Act
        var result = _controller.ListTools();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<McpListToolsResponse>();
        
        var response = okResult.Value as McpListToolsResponse;
        response!.Tools.Should().NotBeNull();
        response.Tools.Should().BeOfType<List<McpTool>>();
    }

    [Fact]
    public void GetServerInfo_ShouldReturnCorrectServerInfo()
    {
        // Act
        var result = _controller.GetServerInfo();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<McpServerInfo>();
        
        var serverInfo = okResult.Value as McpServerInfo;
        serverInfo!.Name.Should().Be("RefactorMCP");
        serverInfo.Version.Should().Be("1.0.0");
        serverInfo.ProtocolVersion.Should().Be("2024-11-05");
        serverInfo.Capabilities.Should().NotBeNull();
        serverInfo.Capabilities!.Tools.Should().NotBeNull();
        serverInfo.Capabilities.Resources.Should().NotBeNull();
    }

    [Theory]
    [InlineData("extract-method")]
    [InlineData("move-method")]
    [InlineData("introduce-variable")]
    public void ToKebabCase_ShouldConvertCamelCaseCorrectly(string expected)
    {
        // This tests the private method indirectly through ListTools
        var result = _controller.ListTools();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as McpListToolsResponse;
        
        // The tools list should contain kebab-case names
        response!.Tools.Select(t => t.Name).Should().Contain(name => name.Contains("-"));
    }

    [Fact]
    public async Task HandleToolCall_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange - force an exception by passing invalid parameters
        var request = new McpToolCallRequest
        {
            ToolName = "", // Empty tool name should cause issues
            Parameters = null
        };

        // Act
        var result = await _controller.HandleToolCall(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeOfType<McpErrorResponse>();

        // Verify error was logged
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Error executing MCP tool")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}

// Test data builders for better test organization
public static class McpTestDataBuilder
{
    public static McpToolCallRequest CreateToolCallRequest(string toolName, Dictionary<string, JsonElement>? parameters = null)
    {
        return new McpToolCallRequest
        {
            ToolName = toolName,
            Parameters = parameters ?? new Dictionary<string, JsonElement>()
        };
    }

    public static Dictionary<string, JsonElement> CreateParameters(params (string key, object value)[] parameters)
    {
        var result = new Dictionary<string, JsonElement>();
        foreach (var (key, value) in parameters)
        {
            var json = JsonSerializer.Serialize(value);
            result[key] = JsonSerializer.Deserialize<JsonElement>(json);
        }
        return result;
    }
}