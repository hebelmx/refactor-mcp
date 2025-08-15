using Microsoft.Extensions.Logging;
using RefactorMCP.Core.Services;
using RefactorMCP.Core.Abstractions;

namespace RefactorMCP.Core.Tests.Services;

public class RefactoringServiceTests
{
    private readonly Mock<ILogger<RefactoringService>> _mockLogger;
    private readonly RefactoringService _service;

    public RefactoringServiceTests()
    {
        _mockLogger = new Mock<ILogger<RefactoringService>>();
        _service = new RefactoringService(_mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteRefactoringAsync_ShouldReturnFailureResult_WhenToolNotImplemented()
    {
        // Arrange
        var request = new RefactoringRequest(
            "test-tool",
            "/path/to/solution.sln",
            new Dictionary<string, object>()
        );

        // Act
        var result = await _service.ExecuteRefactoringAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not yet implemented");
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
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not yet implemented");

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Extracting method")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not yet implemented");

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Moving method")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not yet implemented");
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
        result.Should().Contain("error");
        result.Should().Contain("not yet implemented");
    }

    [Fact]
    public async Task ListAvailableToolsAsync_ShouldReturnExpectedTools()
    {
        // Act
        var tools = await _service.ListAvailableToolsAsync();

        // Assert
        tools.Should().NotBeEmpty();
        tools.Should().Contain("extract-method");
        tools.Should().Contain("move-method");
        tools.Should().Contain("introduce-variable");
        tools.Should().Contain("safe-delete");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ExecuteRefactoringAsync_ShouldHandleInvalidToolName(string? toolName)
    {
        // Arrange
        var request = new RefactoringRequest(
            toolName ?? string.Empty,
            "/path/to/solution.sln",
            new Dictionary<string, object>()
        );

        // Act
        var result = await _service.ExecuteRefactoringAsync(request);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteRefactoringAsync_ShouldHandleException_AndReturnErrorResult()
    {
        // This test would be more relevant once real implementations exist
        // For now, testing the error handling structure
        var request = new RefactoringRequest(
            "test-tool",
            "/path/to/solution.sln",
            new Dictionary<string, object>()
        );

        var result = await _service.ExecuteRefactoringAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }
}