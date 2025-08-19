namespace ExxerFactor.Mcp.Tests.Tools;

public class MoveMultipleMethodsDuplicateTests : TestBase
{
    [Fact]
    public async Task MoveMultipleMethods_DuplicateNames_Fails()
    {
        UnloadSolutionTool.ClearSolutionCache();
        var testFile = Path.Combine(TestOutputPath, "DupMethods.cs");
        var code = @"public class Source { public void A() { } } public class Target { }";
        await TestUtilities.CreateTestFile(testFile, code);
        await LoadSolutionTool.LoadSolution(SolutionPath, null, CancellationToken.None);
        var solution = await ExxerFactoringHelpers.GetOrLoadSolution(SolutionPath);
        var project = solution.Projects.First();
        ExxerFactoringHelpers.AddDocumentToProject(project, testFile);

        var result = await MoveMultipleMethodsTool.MoveMultipleMethodsStatic(
            SolutionPath,
            testFile,
            "Source",
            new[] { "A", "A" },
            "Target");

        Assert.Contains("Duplicate method names", result);
    }
}