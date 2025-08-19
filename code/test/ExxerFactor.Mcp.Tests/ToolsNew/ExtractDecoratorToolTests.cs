using ExxerFactor.Mcp.Tests.Tools;

namespace ExxerFactor.Mcp.Tests.ToolsNew;

public class ExtractDecoratorToolTests : TestBase
{
    [Fact]
    public async Task ExtractDecorator_AddsClass()
    {
        const string initialCode = "public class Greeter { public void Greet(string name){ System.Console.WriteLine(\"Hello {name}\"); } }";

        await LoadSolutionTool.LoadSolution(SolutionPath, null, CancellationToken.None);
        var testFile = Path.Combine(TestOutputPath, "Decorator.cs");
        await TestUtilities.CreateTestFile(testFile, initialCode);

        var result = await ExtractDecoratorTool.ExtractDecorator(
            SolutionPath,
            testFile,
            "Greeter",
            "Greet");

        Assert.Contains("Created decorator", result);
        var text = await File.ReadAllTextAsync(testFile);
        Assert.Contains("GreeterDecorator", text);
    }
}