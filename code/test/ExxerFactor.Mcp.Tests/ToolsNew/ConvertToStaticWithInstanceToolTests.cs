using ExxerFactor.Mcp.Tests.Tools;

namespace ExxerFactor.Mcp.Tests.ToolsNew;

public class ConvertToStaticWithInstanceToolTests : TestBase
{
    [Fact]
    public async Task ConvertToStaticWithInstance_ReturnsSuccess()
    {
        const string initialCode = "public class Calculator { public string GetFormattedNumber(int n){ return $\"{n}\"; } }";

        await LoadSolutionTool.LoadSolution(SolutionPath, null, CancellationToken.None);
        var testFile = Path.Combine(TestOutputPath, "ConvertToStaticInstance.cs");
        await TestUtilities.CreateTestFile(testFile, initialCode);

        var result = await ConvertToStaticWithInstanceTool.ConvertToStaticWithInstance(
            SolutionPath,
            testFile,
            "GetFormattedNumber",
            "instance");

        Assert.Contains("Successfully converted method 'GetFormattedNumber' to static with instance parameter", result);
        var fileContent = await File.ReadAllTextAsync(testFile);
        Assert.Contains("static string GetFormattedNumber", fileContent);
        Assert.Contains("Calculator instance", fileContent);
    }
}