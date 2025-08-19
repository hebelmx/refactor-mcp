namespace ExxerFactor.Mcp.Tests.Tools;

public class FeatureFlagExxerFactorTests : TestBase
{
    [Fact]
    public async Task FeatureFlagExxerFactor_RewritesFile()
    {
        await LoadSolutionTool.LoadSolution(SolutionPath, null, CancellationToken.None);
        var testFile = Path.Combine(TestOutputPath, "FeatureFlag.cs");
        await TestUtilities.CreateTestFile(testFile, TestUtilities.GetSampleCodeForFeatureFlag());

        var result = await FeatureFlagExxerFactorTool.FeatureFlagExxerFactor(
            SolutionPath,
            testFile,
            "CoolFeature");

        Assert.Contains("ExxerFactored feature flag", result);
        var content = await File.ReadAllTextAsync(testFile);
        Assert.Contains("ICoolFeatureStrategy", content);
        Assert.Contains("_coolFeatureStrategy", content);
    }

    [Fact]
    public async Task FeatureFlagExxerFactor_NoFlagFound_Throws()
    {
        await LoadSolutionTool.LoadSolution(SolutionPath, null, CancellationToken.None);
        var testFile = Path.Combine(TestOutputPath, "FeatureFlagMissing.cs");
        await TestUtilities.CreateTestFile(testFile, TestUtilities.GetSampleCodeForFeatureFlag());

        await Assert.ThrowsAsync<McpException>(async () =>
            await FeatureFlagExxerFactorTool.FeatureFlagExxerFactor(
                SolutionPath,
                testFile,
                "Other"));
    }
}