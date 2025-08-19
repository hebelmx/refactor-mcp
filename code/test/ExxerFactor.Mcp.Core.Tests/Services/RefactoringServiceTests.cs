using ExxerFactor.Mcp.Core.Abstractions;
using ExxerFactor.Mcp.Core.Services;

namespace ExxerFactor.Mcp.Core.Tests.Services;

public class ExxerFactoringServiceTests
{
    private readonly ILogger<ExxerFactoringService> _mockLogger;
    private readonly ExxerFactoringService _service;

    public ExxerFactoringServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ExxerFactoringService>>();
        _service = new ExxerFactoringService(_mockLogger);
    }

    [Fact]
    public async Task ExecuteExxerFactoringAsync_ShouldReturnFailureResult_WhenToolNotImplemented()
    {
        // Arrange
        var request = new ExxerFactoringRequest(
            "test-tool",
            "/path/to/solution.sln",
            new Dictionary<string, object>()
        );

        // Act
        var result = await _service.ExecuteExxerFactoringAsync(request);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldContain("not yet implemented");
    }

    [Fact]
    public async Task ExtractMethodAsync_ShouldReturnFailureResult_WhenNotImplemented()
    {
        // Arrange
        var solutionPath = "/path/to/solution.sln";
        var filePath = "/path/to/file.cs";
        var startLine = 10;
        var endLine = 15;
        var methodName = "ExtractedMethod";

        // Act
        var result = await _service.ExtractMethodAsync(solutionPath, filePath, startLine, endLine, methodName);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldContain("not yet implemented");

        // Verify logging
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Extracting method")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task MoveMethodAsync_ShouldReturnFailureResult_WhenNotImplemented()
    {
        // Arrange
        var solutionPath = "/path/to/solution.sln";
        var sourceFilePath = "/path/to/source.cs";
        var methodName = "MethodToMove";
        var targetClassName = "TargetClass";

        // Act
        var result = await _service.MoveMethodAsync(solutionPath, sourceFilePath, methodName, targetClassName);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldContain("not yet implemented");

        // Verify logging
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Moving method")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task IntroduceVariableAsync_ShouldReturnFailureResult_WhenNotImplemented()
    {
        // Arrange
        var solutionPath = "/path/to/solution.sln";
        var filePath = "/path/to/file.cs";
        var line = 10;
        var column = 5;
        var variableName = "newVariable";

        // Act
        var result = await _service.IntroduceVariableAsync(solutionPath, filePath, line, column, variableName);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldContain("not yet implemented");
    }

    [Fact]
    public async Task GetMetricsAsync_ShouldReturnErrorJson_WhenNotImplemented()
    {
        // Arrange
        var solutionPath = "/path/to/solution.sln";
        var path = "/path/to/file.cs";

        // Act
        var result = await _service.GetMetricsAsync(solutionPath, path);

        // Assert
        result.ShouldContain("error");
        result.ShouldContain("not yet implemented");
    }

    [Fact]
    public async Task ListAvailableToolsAsync_ShouldReturnExpectedTools()
    {
        // Act
        var tools = await _service.ListAvailableToolsAsync();

        // Assert
        tools.ShouldNotBeEmpty();
        tools.ShouldContain("extract-method");
        tools.ShouldContain("move-method");
        tools.ShouldContain("introduce-variable");
        tools.ShouldContain("safe-delete");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteExxerFactoringAsync_ShouldHandleInvalidToolName(string? toolName)
    {
        // Arrange
        var request = new ExxerFactoringRequest(
            toolName ?? string.Empty,
            "/path/to/solution.sln",
            new Dictionary<string, object>()
        );

        // Act
        var result = await _service.ExecuteExxerFactoringAsync(request);

        // Assert
        result.Success.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteExxerFactoringAsync_ShouldHandleException_AndReturnErrorResult()
    {
        // This test would be more relevant once real implementations exist
        // For now, testing the error handling structure
        var request = new ExxerFactoringRequest(
            "test-tool",
            "/path/to/solution.sln",
            new Dictionary<string, object>()
        );

        var result = await _service.ExecuteExxerFactoringAsync(request);

        result.ShouldNotBeNull();
        result.Success.ShouldBeFalse();
        result.Message.ShouldNotBeNullOrEmpty();
    }
}