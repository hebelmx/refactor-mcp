using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using ExxerFactor.Mcp.Core.Move;
using ExxerFactor.Mcp.Core.SyntaxWalkers;

namespace ExxerFactor.Mcp.Core.Tools;

[McpServerToolType]
public static partial class MoveMultipleMethodsTool
{
    private static async Task<(string message, Document updatedDocument)> MoveSingleMethod(
        Document document,
        string sourceClass,
        string methodName,
        bool isStatic,
        bool ctorInjection,
        string targetClass,
        string accessMember,
        string accessMemberType,
        string targetPath,
        CancellationToken cancellationToken)
    {
        string message;
        if (isStatic)
        {
            message = await MoveMethodFileService.MoveStaticMethodInFile(document.FilePath!, methodName, targetClass, targetPath, progress: null, cancellationToken);
        }
        else
        {
            var ctor = ctorInjection ? new[] { "this" } : Array.Empty<string>();
            var param = ctorInjection ? Array.Empty<string>() : new[] { "this" };
            message = await MoveMethodFileService.MoveInstanceMethodInFile(
                document.FilePath!,
                sourceClass,
                methodName,
                ctor,
                param,
                targetClass,
                accessMember,
                accessMemberType,
                targetPath,
                progress: null,
                cancellationToken);
        }

        var (newText, _) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(document.FilePath!, cancellationToken);
        var newRoot = await CSharpSyntaxTree.ParseText(newText).GetRootAsync(cancellationToken);
        var solution = document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newRoot);

        var project = solution.GetProject(document.Project.Id)!;
        var targetDocument = project.Documents.FirstOrDefault(d => d.FilePath == targetPath);
        if (targetDocument == null)
        {
            var (targetText, targetEncoding) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(targetPath, cancellationToken);
            var targetSource = SourceText.From(targetText, targetEncoding);
            targetDocument = project.AddDocument(Path.GetFileName(targetPath), targetSource, filePath: targetPath);
            solution = targetDocument.Project.Solution;
        }
        else
        {
            var (targetText, targetEncoding) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(targetPath, cancellationToken);
            var targetSource = SourceText.From(targetText, targetEncoding);
            solution = solution.WithDocumentText(targetDocument.Id, targetSource);
        }

        var updatedDoc = solution.GetDocument(document.Id)!;
        ExxerFactoringHelpers.UpdateSolutionCache(updatedDoc);

        return (message, updatedDoc);
    }

    private static Task<string> MoveMultipleMethodsInternal(
        string solutionPath,
        string filePath,
        string sourceClass,
        string[] methodNames,
        string targetClass,
        string? targetFilePath,
        bool ctorInjection,
        CancellationToken cancellationToken)
        => MoveMultipleMethodsCore(solutionPath, filePath, sourceClass, methodNames, targetClass, targetFilePath, ctorInjection, cancellationToken);

    private static async Task<string> MoveMultipleMethodsCore(
        string solutionPath,
        string filePath,
        string sourceClass,
        string[] methodNames,
        string targetClass,
        string? targetFilePath,
        bool ctorInjection,
        CancellationToken cancellationToken)
    {
        if (methodNames.Length == 0)
            throw new McpException("Error: No method names provided");

        var dupes = methodNames.GroupBy(m => m).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dupes.Count > 0)
            return $"Error: Duplicate method names are not supported: {string.Join(", ", dupes)}";

        foreach (var methodName in methodNames)
            MoveMethodTool.EnsureNotAlreadyMoved(filePath, methodName);

        var solution = await ExxerFactoringHelpers.GetOrLoadSolution(solutionPath, cancellationToken);
        var document = ExxerFactoringHelpers.GetDocumentByPath(solution, filePath);
        if (document == null)
            throw new McpException("Error: Could not find document in solution and AST fallback is disabled.");

        var root = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new McpException("Error: Could not get syntax root");

        var collector = new ClassCollectorWalker();
        collector.Visit(root);
        if (!collector.Classes.TryGetValue(sourceClass, out var sourceClassNode))
            throw new McpException($"Error: Source class '{sourceClass}' not found");

        var visitor = new MethodAndMemberVisitor();
        visitor.Visit(sourceClassNode);
        var accessMemberName = MoveMethodAst.GenerateAccessMemberName(visitor.Members.Keys, targetClass);

        var staticWalker = new MethodStaticWalker(methodNames);
        staticWalker.Visit(sourceClassNode);

        var memberWalker = new AccessMemberTypeWalker(accessMemberName);
        memberWalker.Visit(sourceClassNode);
        var instanceMemberType = memberWalker.MemberType ?? "field";

        var isStatic = new bool[methodNames.Length];
        var accessMemberTypes = new string[methodNames.Length];
        for (int i = 0; i < methodNames.Length; i++)
        {
            var methodName = methodNames[i];
            if (!staticWalker.IsStaticMap.TryGetValue(methodName, out var isStaticMethod))
                return $"Error: No method named '{methodName}' in class '{sourceClass}'";

            isStatic[i] = isStaticMethod;
            accessMemberTypes[i] = isStaticMethod ? string.Empty : instanceMemberType;
        }

        var orderedIndices = OrderOperations(root, Enumerable.Repeat(sourceClass, methodNames.Length).ToArray(), methodNames);

        var results = new List<string>();
        var moved = new List<(string file, string method)>();
        var currentDoc = document;
        var targetPath = targetFilePath ?? Path.Combine(Path.GetDirectoryName(document.FilePath!)!, $"{targetClass}.cs");

        foreach (var idx in orderedIndices)
        {
            try
            {
                var result = await MoveSingleMethod(
                    currentDoc,
                    sourceClass,
                    methodNames[idx],
                    isStatic[idx],
                    ctorInjection,
                    targetClass,
                    accessMemberName,
                    accessMemberTypes[idx],
                    targetPath,
                    cancellationToken);
                currentDoc = result.updatedDocument;
                moved.Add((document.FilePath!, methodNames[idx]));
                results.Add(result.message);
            }
            catch (Exception ex)
            {
                results.Add($"Error moving method '{methodNames[idx]}': {ex.Message}\nStack Trace:\n{ex.StackTrace}");
            }
        }

        foreach (var (file, method) in moved)
            MoveMethodTool.MarkMoved(file, method);

        return string.Join("\n", results);
    }

    // Solution/Document operations that use the AST layer

    [McpServerTool, Description("Move multiple methods to a target class and transform them to static with an injected 'this' parameter.")]
    public static Task<string> MoveMultipleMethodsStatic(
        [Description("Absolute path to the solution file (.sln)")] string solutionPath,
        [Description("Path to the C# file containing the methods")] string filePath,
        [Description("Name of the source class containing the methods")] string sourceClass,
        [Description("Names of the methods to move")] string[] methodNames,
        [Description("Name of the target class")] string targetClass,
        [Description("Path to the target file (optional, target class will be automatically created if it doesnt exist or its unspecified)")] string? targetFilePath = null,
        CancellationToken cancellationToken = default)
        => MoveMultipleMethodsInternal(solutionPath, filePath, sourceClass, methodNames, targetClass, targetFilePath, false, cancellationToken);

    [McpServerTool, Description("Move multiple methods and keep them as instance methods in the target class. The source instance is injected via the constructor if needed.")]
    public static Task<string> MoveMultipleMethodsInstance(
        [Description("Absolute path to the solution file (.sln)")] string solutionPath,
        [Description("Path to the C# file containing the methods")] string filePath,
        [Description("Name of the source class containing the methods")] string sourceClass,
        [Description("Names of the methods to move")] string[] methodNames,
        [Description("Name of the target class")] string targetClass,
        [Description("Path to the target file (optional, target class will be automatically created if it doesnt exist or its unspecified)")] string? targetFilePath = null,
        CancellationToken cancellationToken = default)
        => MoveMultipleMethodsInternal(solutionPath, filePath, sourceClass, methodNames, targetClass, targetFilePath, true, cancellationToken);

    // ===== HELPER METHODS =====

    public static Dictionary<string, HashSet<string>> BuildDependencies(
        SyntaxNode sourceRoot,
        string[] sourceClasses,
        string[] methodNames)
    {
        // Build map keyed by "Class.Method" to support duplicate method names in different classes
        var opSet = sourceClasses.Zip(methodNames, (c, m) => $"{c}.{m}").ToHashSet();
        var collector = new MethodCollectorWalker(opSet);
        collector.Visit(sourceRoot);
        var map = collector.Methods;

        var methodNameSet = methodNames.ToHashSet();
        var deps = new Dictionary<string, HashSet<string>>();

        for (int i = 0; i < sourceClasses.Length; i++)
        {
            var key = $"{sourceClasses[i]}.{methodNames[i]}";
            if (!map.TryGetValue(key, out var method))
            {
                deps[key] = new HashSet<string>();
                continue;
            }

            var walker = new MethodDependencyWalker(methodNameSet);
            walker.Visit(method);

            var called = walker.Dependencies
                .Select(name => $"{sourceClasses[i]}.{name}")
                .Where(n => map.ContainsKey(n))
                .ToHashSet();

            deps[key] = called;
        }

        return deps;
    }

    public static List<int> OrderOperations(
        SyntaxNode sourceRoot,
        string[] sourceClasses,
        string[] methodNames)
    {
        var deps = BuildDependencies(sourceRoot, sourceClasses, methodNames);
        var indices = Enumerable.Range(0, sourceClasses.Length).ToList();
        return indices.OrderBy(i => deps.TryGetValue($"{sourceClasses[i]}.{methodNames[i]}", out var d) ? d.Count : 0).ToList();
    }
}