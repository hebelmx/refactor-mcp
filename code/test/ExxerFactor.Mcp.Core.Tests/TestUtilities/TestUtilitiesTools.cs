namespace ExxerFactor.Mcp.Core.Tests.TestUtilities;

/// <summary>
/// Test utilities for creating test data and common assertions
/// </summary>
public static class TestUtilitiesTools
{
    public static string CreateTempDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "ExxerFactor.Mcp.Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);
        return tempPath;
    }

    public static string CreateTempFile(string content, string? fileName = null)
    {
        var tempDir = CreateTempDirectory();
        var filePath = Path.Combine(tempDir, fileName ?? "test.cs");
        File.WriteAllText(filePath, content);
        return filePath;
    }

    public static void CleanupTempDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public static string CreateSampleCSharpClass(string className = "TestClass", string[] methods = null!)
    {
        methods ??= new[] { "TestMethod" };
        var methodsCode = string.Join("\n    ", methods.Select(m => $"public void {m}() {{ }}"));

        return $@"
using System;

namespace TestNamespace
{{
    public class {className}
    {{
        {methodsCode}
    }}
}}";
    }

    public static string CreateSampleSolution(string solutionName = "TestSolution")
    {
        return $@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{solutionName}"", ""{solutionName}.csproj"", ""{{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}}""
EndProject
Global
    GlobalSection(SolutionConfigurationPlatforms) = preSolution
        Debug|Any CPU = Debug|Any CPU
        Release|Any CPU = Release|Any CPU
    EndGlobalSection
EndGlobal";
    }
}