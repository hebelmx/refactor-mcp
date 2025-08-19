namespace ExxerFactor.Mcp.Core.Tests.TestUtilities;

/// <summary>
/// Utility class for creating test files and solutions for ExxerFactoring tests.
/// </summary>
public static class TestFileUtilities
{
    /// <summary>
    /// Creates a temporary test directory with a unique name.
    /// </summary>
    /// <param name="prefix">Prefix for the directory name.</param>
    /// <returns>Path to the created directory.</returns>
    public static string CreateTestDirectory(string prefix = "ExxerFactoringTest")
    {
        var testDirectory = Path.Combine(Path.GetTempPath(), $"{prefix}_{Guid.NewGuid()}");
        Directory.CreateDirectory(testDirectory);
        return testDirectory;
    }

    /// <summary>
    /// Creates a simple test solution with a single project and class.
    /// </summary>
    /// <param name="testDirectory">Directory to create the solution in.</param>
    /// <param name="className">Name of the class to create.</param>
    /// <param name="classContent">Content of the class.</param>
    /// <returns>Path to the created solution file.</returns>
    public static string CreateTestSolution(string testDirectory, string className = "TestClass", string classContent = "")
    {
        var solutionPath = Path.Combine(testDirectory, "TestSolution.sln");
        var projectPath = Path.Combine(testDirectory, "TestProject.csproj");
        var classPath = Path.Combine(testDirectory, $"{className}.cs");

        // Create solution file
        var solutionContent = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""TestProject"", ""TestProject.csproj"", ""{12345678-1234-1234-1234-123456789012}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{12345678-1234-1234-1234-123456789012}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{12345678-1234-1234-1234-123456789012}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{12345678-1234-1234-1234-123456789012}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{12345678-1234-1234-1234-123456789012}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal";

        // Create project file
        var projectContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>";

        // Create default class content if none provided
        if (string.IsNullOrEmpty(classContent))
        {
            classContent = @"
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod()
        {
            Console.WriteLine(""Hello World"");
        }
    }
}";
        }

        File.WriteAllText(solutionPath, solutionContent);
        File.WriteAllText(projectPath, projectContent);
        File.WriteAllText(classPath, classContent);

        return solutionPath;
    }

    /// <summary>
    /// Creates a test C# file with the specified content.
    /// </summary>
    /// <param name="directory">Directory to create the file in.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="content">Content of the file.</param>
    /// <returns>Path to the created file.</returns>
    public static string CreateTestFile(string directory, string fileName, string content)
    {
        var filePath = Path.Combine(directory, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    /// <summary>
    /// Creates a test class with a static method.
    /// </summary>
    /// <param name="className">Name of the class.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="methodBody">Body of the method.</param>
    /// <returns>C# source code for the class.</returns>
    public static string CreateStaticMethodClass(string className, string methodName, string methodBody = "")
    {
        if (string.IsNullOrEmpty(methodBody))
        {
            methodBody = "Console.WriteLine(\"Hello World\");";
        }

        return $@"
using System;

namespace TestNamespace
{{
    public class {className}
    {{
        public static void {methodName}()
        {{
            {methodBody}
        }}
    }}
}}";
    }

    /// <summary>
    /// Creates a test class with an instance method.
    /// </summary>
    /// <param name="className">Name of the class.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="parameters">Parameters for the method.</param>
    /// <param name="methodBody">Body of the method.</param>
    /// <returns>C# source code for the class.</returns>
    public static string CreateInstanceMethodClass(string className, string methodName, string parameters = "", string methodBody = "")
    {
        if (string.IsNullOrEmpty(methodBody))
        {
            methodBody = "Console.WriteLine(\"Hello World\");";
        }

        return $@"
using System;

namespace TestNamespace
{{
    public class {className}
    {{
        public void {methodName}({parameters})
        {{
            {methodBody}
        }}
    }}
}}";
    }

    /// <summary>
    /// Creates a test class with multiple methods.
    /// </summary>
    /// <param name="className">Name of the class.</param>
    /// <param name="methods">Dictionary of method names and their bodies.</param>
    /// <returns>C# source code for the class.</returns>
    public static string CreateMultiMethodClass(string className, Dictionary<string, string> methods)
    {
        var methodDefinitions = string.Join("\n\n        ", methods.Select(kvp =>
            $"public void {kvp.Key}()\n        {{\n            {kvp.Value}\n        }}"));

        return $@"
using System;

namespace TestNamespace
{{
    public class {className}
    {{
        {methodDefinitions}
    }}
}}";
    }

    /// <summary>
    /// Creates a test class with unused using statements.
    /// </summary>
    /// <param name="className">Name of the class.</param>
    /// <param name="usedUsings">List of using statements that should be used.</param>
    /// <param name="unusedUsings">List of using statements that should be unused.</param>
    /// <returns>C# source code for the class.</returns>
    public static string CreateClassWithUsings(string className, IEnumerable<string> usedUsings, IEnumerable<string> unusedUsings)
    {
        var allUsings = usedUsings.Concat(unusedUsings);
        var usingStatements = string.Join("\n", allUsings.Select(u => $"using {u};"));

        var usedCode = string.Join("\n            ", usedUsings.Select(u =>
        {
            var parts = u.Split('.');
            var lastPart = parts[^1];
            return $"var {lastPart.ToLower()} = new {lastPart}();";
        }));

        return $@"
{usingStatements}

namespace TestNamespace
{{
    public class {className}
    {{
        public void TestMethod()
        {{
            {usedCode}
        }}
    }}
}}";
    }

    /// <summary>
    /// Creates a test class with constructor injection dependencies.
    /// </summary>
    /// <param name="className">Name of the class.</param>
    /// <param name="dependencies">Dictionary of dependency names and their types.</param>
    /// <param name="methodName">Name of the method that uses dependencies.</param>
    /// <returns>C# source code for the class.</returns>
    public static string CreateClassWithDependencies(string className, Dictionary<string, string> dependencies, string methodName = "TestMethod")
    {
        var constructorParams = string.Join(", ", dependencies.Select(d => $"{d.Value} {d.Key}"));
        var constructorAssignments = string.Join("\n            ", dependencies.Select(d => $"_{d.Key} = {d.Key};"));
        var fieldDeclarations = string.Join("\n        ", dependencies.Select(d => $"private readonly {d.Value} _{d.Key};"));
        var methodUsages = string.Join("\n            ", dependencies.Select(d => $"_{d.Key}.DoSomething();"));

        return $@"
using System;

namespace TestNamespace
{{
    public class {className}
    {{
        {fieldDeclarations}

        public {className}({constructorParams})
        {{
            {constructorAssignments}
        }}

        public void {methodName}()
        {{
            {methodUsages}
        }}
    }}
}}";
    }

    /// <summary>
    /// Parses C# source code and returns the syntax tree.
    /// </summary>
    /// <param name="sourceCode">C# source code to parse.</param>
    /// <returns>Parsed syntax tree.</returns>
    public static SyntaxTree ParseSourceCode(string sourceCode)
    {
        return CSharpSyntaxTree.ParseText(sourceCode);
    }

    /// <summary>
    /// Gets the first class declaration from a syntax tree.
    /// </summary>
    /// <param name="syntaxTree">Syntax tree to search.</param>
    /// <returns>First class declaration found.</returns>
    public static ClassDeclarationSyntax? GetFirstClass(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
    }

    /// <summary>
    /// Gets the first method declaration from a syntax tree.
    /// </summary>
    /// <param name="syntaxTree">Syntax tree to search.</param>
    /// <returns>First method declaration found.</returns>
    public static MethodDeclarationSyntax? GetFirstMethod(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
    }

    /// <summary>
    /// Gets all method declarations from a syntax tree.
    /// </summary>
    /// <param name="syntaxTree">Syntax tree to search.</param>
    /// <returns>All method declarations found.</returns>
    public static IEnumerable<MethodDeclarationSyntax> GetAllMethods(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
    }

    /// <summary>
    /// Gets all using directives from a syntax tree.
    /// </summary>
    /// <param name="syntaxTree">Syntax tree to search.</param>
    /// <returns>All using directives found.</returns>
    public static IEnumerable<UsingDirectiveSyntax> GetAllUsings(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>();
    }

    /// <summary>
    /// Creates a compilation with the specified syntax trees and references.
    /// </summary>
    /// <param name="syntaxTrees">Syntax trees to include in the compilation.</param>
    /// <param name="references">Additional metadata references.</param>
    /// <returns>Created compilation.</returns>
    public static Compilation CreateCompilation(IEnumerable<SyntaxTree> syntaxTrees, IEnumerable<MetadataReference>? references = null)
    {
        var defaultReferences = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
        };

        var allReferences = references != null ? defaultReferences.Concat(references) : defaultReferences;

        return CSharpCompilation.Create("TestCompilation")
            .AddReferences(allReferences)
            .AddSyntaxTrees(syntaxTrees)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>
    /// Cleans up a test directory.
    /// </summary>
    /// <param name="directory">Directory to clean up.</param>
    public static void CleanupTestDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    /// <summary>
    /// Creates a test class that implements IDisposable for automatic cleanup.
    /// </summary>
    public class TestDirectoryScope : IDisposable
    {
        public string Directory { get; }

        public TestDirectoryScope(string prefix = "ExxerFactoringTest")
        {
            Directory = CreateTestDirectory(prefix);
        }

        public void Dispose()
        {
            CleanupTestDirectory(Directory);
        }
    }
}