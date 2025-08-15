using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RefactorMCP.Core.Extensions;

namespace RefactorMCP.Core.Tests;

/// <summary>
/// Base class for RefactorMCP.Core tests providing common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected ServiceProvider ServiceProvider { get; }
    protected IServiceScope Scope { get; }

    protected TestBase()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        Scope = ServiceProvider.CreateScope();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(GetTestConfiguration())
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Add RefactorMCP Core services
        services.AddRefactorMcpCore();
    }

    protected virtual Dictionary<string, string?> GetTestConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["Logging:LogLevel:Default"] = "Debug",
            ["RefactorMCP:TestMode"] = "true"
        };
    }

    protected T GetService<T>() where T : notnull
    {
        return Scope.ServiceProvider.GetRequiredService<T>();
    }

    protected T? GetOptionalService<T>()
    {
        return Scope.ServiceProvider.GetService<T>();
    }

    protected ILogger<T> GetLogger<T>()
    {
        return GetService<ILogger<T>>();
    }

    public virtual void Dispose()
    {
        Scope?.Dispose();
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Test utilities for creating test data and common assertions
/// </summary>
public static class TestUtilities
{
    public static string CreateTempDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "RefactorMCP.Tests", Guid.NewGuid().ToString());
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