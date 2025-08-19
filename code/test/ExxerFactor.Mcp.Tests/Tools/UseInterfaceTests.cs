namespace ExxerFactor.Mcp.Tests.Tools;

public class UseInterfaceTests : TestBase
{
    [Fact]
    public async Task UseInterface_ChangesParameterType()
    {
        await LoadSolutionTool.LoadSolution(SolutionPath, null, CancellationToken.None);
        var testFile = Path.Combine(TestOutputPath, "UseInterface.cs");
        await TestUtilities.CreateTestFile(testFile, TestUtilities.GetSampleCodeForUseInterface());

        var result = await UseInterfaceTool.UseInterface(
            SolutionPath,
            testFile,
            "DoWork",
            "writer",
            "IWriter");

        Assert.Contains("Successfully changed parameter", result);
        var fileContent = await File.ReadAllTextAsync(testFile);
        Assert.Contains("DoWork(IWriter writer)", fileContent);
    }
}