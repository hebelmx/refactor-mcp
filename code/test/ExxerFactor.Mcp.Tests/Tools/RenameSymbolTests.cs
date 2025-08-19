namespace ExxerFactor.Mcp.Tests.Tools;

public class RenameSymbolTests : TestBase
{
    [Fact]
    public async Task RenameSymbol_Field_RenamesAllReferences()
    {
        UnloadSolutionTool.ClearSolutionCache();
        await LoadSolutionTool.LoadSolution(SolutionPath, null, CancellationToken.None);
        var testFile = Path.Combine(TestOutputPath, "RenameSymbol.cs");
        await TestUtilities.CreateTestFile(testFile, TestUtilities.GetSampleCodeForRenameSymbol());
        var solution = await ExxerFactoringHelpers.GetOrLoadSolution(SolutionPath);
        var project = solution.Projects.First();
        ExxerFactoringHelpers.AddDocumentToProject(project, testFile);

        var result = await RenameSymbolTool.RenameSymbol(
            SolutionPath,
            testFile,
            "numbers",
            "values");

        Assert.Contains("Successfully renamed", result);
        var content = await File.ReadAllTextAsync(testFile);
        Assert.DoesNotContain("List<int> numbers", content);
        Assert.DoesNotContain("numbers.Add", content);
        Assert.Contains("List<int> values", content);
        Assert.Contains("values.Add", content);
    }

    [Fact]
    public async Task RenameSymbol_InvalidName_ReturnsError()
    {
        UnloadSolutionTool.ClearSolutionCache();
        await LoadSolutionTool.LoadSolution(SolutionPath, null, CancellationToken.None);
        var testFile = Path.Combine(TestOutputPath, "RenameInvalid.cs");
        await TestUtilities.CreateTestFile(testFile, TestUtilities.GetSampleCodeForRenameSymbol());
        var solution = await ExxerFactoringHelpers.GetOrLoadSolution(SolutionPath);
        var project = solution.Projects.First();
        ExxerFactoringHelpers.AddDocumentToProject(project, testFile);

        await Assert.ThrowsAsync<McpException>(() =>
            RenameSymbolTool.RenameSymbol(
                SolutionPath,
                testFile,
                "missing",
                "newName"));
    }
}