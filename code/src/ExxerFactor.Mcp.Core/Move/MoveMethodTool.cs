using System.ComponentModel;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using ExxerFactor.Mcp.Core.SyntaxWalkers;
using ExxerFactor.Mcp.Core.Tools;

namespace ExxerFactor.Mcp.Core.Move;

[McpServerToolType]
public static class MoveMethodTool
{
    private static readonly HashSet<string> _movedMethods = new();

    private static string GetKey(string filePath, string methodName) =>
        $"{Path.GetFullPath(filePath)}::{methodName}";

    public static void EnsureNotAlreadyMoved(string filePath, string methodName)
    {
        if (_movedMethods.Contains(GetKey(filePath, methodName)))
        {
            throw new McpException(
                $"Error: Method '{methodName}' appears to have been moved already during this session. " +
                "Consider using inline-method if you want to remove the wrapper.");
        }
    }

    public static void MarkMoved(string filePath, string methodName)
        => _movedMethods.Add(GetKey(filePath, methodName));

    [McpServerTool, Description("Clear the record of moved methods so they can be moved again. Do not use unless explicitly asked to.")]
    public static string ResetMoveHistory()
    {
        _movedMethods.Clear();
        return "Cleared move history";
    }

    [McpServerTool, Description("Move a static method to another class (preferred for large C# file ExxerFactoring). " +
        "Leaves a delegating method in the original class to preserve the interface." +
        "The target class will be automatically created if it doesn't exist.")]
    public static async Task<string> MoveStaticMethod(
        [Description("Absolute path to the solution file (.sln)")] string solutionPath,
        [Description("Path to the C# file containing the method")] string filePath,
        [Description("Name of the static method to move")] string methodName,
        [Description("Name of the target class")] string targetClass,
        [Description("Path to the target file (optional, will create if doesn't exist or unspecified)")] string? targetFilePath = null,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureNotAlreadyMoved(filePath, methodName);
            MoveMethodFileService.ValidateFileExists(filePath);

            var moveContext = await PrepareStaticMethodMove(filePath, targetFilePath, targetClass, cancellationToken);
            var solution = await ExxerFactoringHelpers.GetOrLoadSolution(solutionPath, cancellationToken);
            var duplicateDoc = await ExxerFactoringHelpers.FindClassInSolution(
                solution,
                targetClass,
                filePath,
                moveContext.TargetPath);
            if (duplicateDoc != null)
                throw new McpException($"Error: Class {targetClass} already exists in {duplicateDoc.FilePath}");
            var method = ExtractStaticMethodFromSource(moveContext.SourceRoot, methodName);
            var updatedSources = await UpdateSourceAndTargetForStaticMove(moveContext, method, cancellationToken);
            await WriteStaticMethodMoveResults(moveContext, updatedSources, progress, cancellationToken);
            MarkMoved(filePath, methodName);

            return $"Successfully moved static method '{methodName}' to {targetClass} in {moveContext.TargetPath}. A delegate method remains in the original class to preserve the interface.";
        }
        catch (Exception ex)
        {
            throw new McpException($"Error moving static method: {ex.Message}", ex);
        }
    }

    private class StaticMethodMoveContext
    {
        public string SourcePath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public bool SameFile { get; set; }
        public SyntaxNode SourceRoot { get; set; } = null!;
        public List<UsingDirectiveSyntax> SourceUsings { get; set; } = new();
        public string TargetClassName { get; set; } = string.Empty;
        public Encoding SourceEncoding { get; set; } = Encoding.UTF8;
        public string? Namespace { get; set; }
    }

    private class SourceAndTargetRoots
    {
        public SyntaxNode UpdatedSourceRoot { get; set; } = null!;
        public SyntaxNode UpdatedTargetRoot { get; set; } = null!;
    }

    private static async Task<StaticMethodMoveContext> PrepareStaticMethodMove(
        string filePath,
        string? targetFilePath,
        string targetClass,
        CancellationToken cancellationToken)
    {
        var (sourceText, sourceEncoding) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(filePath, cancellationToken);
        var directoryName = Path.GetDirectoryName(filePath);
        if (directoryName == null)
            throw new InvalidOperationException($"Could not determine directory for file {filePath}");
        var targetPath = targetFilePath ?? Path.Combine(directoryName, $"{targetClass}.cs");

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
        var syntaxRoot = await syntaxTree.GetRootAsync(cancellationToken);
        var sourceUsings = syntaxRoot.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
        var ns = syntaxRoot.DescendantNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault()?.Name.ToString();

        return new StaticMethodMoveContext
        {
            SourcePath = filePath,
            TargetPath = targetPath,
            SameFile = targetPath == filePath,
            SourceRoot = syntaxRoot,
            SourceUsings = sourceUsings,
            TargetClassName = targetClass,
            SourceEncoding = sourceEncoding,
            Namespace = ns
        };
    }

    private static MethodDeclarationSyntax ExtractStaticMethodFromSource(SyntaxNode sourceRoot, string methodName)
    {
        var method = sourceRoot.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == methodName &&
                                m.Modifiers.Any(SyntaxKind.StaticKeyword));

        if (method == null)
            throw new McpException($"Error: Static method '{methodName}' not found");

        return method;
    }

    private static async Task<SourceAndTargetRoots> UpdateSourceAndTargetForStaticMove(
        StaticMethodMoveContext context,
        MethodDeclarationSyntax method,
        CancellationToken cancellationToken)
    {
        var newSourceRoot = context.SourceRoot.RemoveNode(method, SyntaxRemoveOptions.KeepNoTrivia);
        var targetRoot = await PrepareTargetRootForStaticMove(context, cancellationToken);
        var updatedTargetRoot = MoveMethodAst.AddMethodToTargetClass(targetRoot, context.TargetClassName, method, context.Namespace);

        if (newSourceRoot == null)
            throw new InvalidOperationException("Could not remove method from source root");
        return new SourceAndTargetRoots
        {
            UpdatedSourceRoot = newSourceRoot,
            UpdatedTargetRoot = updatedTargetRoot
        };
    }

    private static async Task<SyntaxNode> PrepareTargetRootForStaticMove(
        StaticMethodMoveContext context,
        CancellationToken cancellationToken)
    {
        SyntaxNode targetRoot;

        if (context.SameFile)
        {
            targetRoot = context.SourceRoot;
        }
        else if (File.Exists(context.TargetPath))
        {
            var (targetText, _) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(context.TargetPath, cancellationToken);
            targetRoot = await CSharpSyntaxTree.ParseText(targetText).GetRootAsync(cancellationToken);
        }
        else
        {
            targetRoot = SyntaxFactory.CompilationUnit();
        }

        return PropagateUsingsToTarget(context, targetRoot);
    }

    private static SyntaxNode PropagateUsingsToTarget(StaticMethodMoveContext context, SyntaxNode targetRoot)
    {
        var targetCompilationUnit = targetRoot as CompilationUnitSyntax ?? throw new InvalidOperationException("Expected compilation unit");
        var targetUsingsWithName = targetCompilationUnit.Usings.Where(u => u.Name != null).ToList();
        var targetUsingNames = targetUsingsWithName
            .Select(u => u.Name!.ToString())
            .ToHashSet();

        var sourceUsingsWithName = context.SourceUsings.Where(u => u.Name != null).ToList();
        var missingUsings = sourceUsingsWithName
            .Where(u => !targetUsingNames.Contains(u.Name!.ToString()))
            .Where(u => context.Namespace == null || u.Name!.ToString() != context.Namespace)
            .ToArray();

        if (missingUsings.Length > 0)
        {
            var validUsings = missingUsings.Where(u => u != null).ToArray();
            if (validUsings.Length > 0)
            {
                targetCompilationUnit = targetCompilationUnit.AddUsings(validUsings);
                return targetCompilationUnit;
            }
        }

        return targetRoot;
    }

    private static async Task WriteStaticMethodMoveResults(
        StaticMethodMoveContext context,
        SourceAndTargetRoots updatedRoots,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var formattedTarget = Formatter.Format(updatedRoots.UpdatedTargetRoot, ExxerFactoringHelpers.SharedWorkspace);

        if (!context.SameFile)
        {
            var formattedSource = Formatter.Format(updatedRoots.UpdatedSourceRoot, ExxerFactoringHelpers.SharedWorkspace);
            await File.WriteAllTextAsync(context.SourcePath, formattedSource.ToFullString(), context.SourceEncoding, cancellationToken);
            progress?.Report(context.SourcePath);
        }

        var targetDirectory = Path.GetDirectoryName(context.TargetPath);
        if (targetDirectory != null)
            Directory.CreateDirectory(targetDirectory);
        var targetEncoding = File.Exists(context.TargetPath)
            ? await ExxerFactoringHelpers.GetFileEncodingAsync(context.TargetPath, cancellationToken)
            : context.SourceEncoding;
        await File.WriteAllTextAsync(context.TargetPath, formattedTarget.ToFullString(), targetEncoding, cancellationToken);
        progress?.Report(context.TargetPath);
    }

    [McpServerTool, Description("Move one or more instance methods to another class (preferred for large C# file ExxerFactoring). " +
        "Each original method is replaced with a wrapper that calls the moved version to maintain the public API." +
        "The target class will be automatically created if it doesn't exist.")]
    public static async Task<string> MoveInstanceMethod(
        [Description("Absolute path to the solution file (.sln)")] string solutionPath,
        [Description("Path to the C# file containing the method")] string filePath,
        [Description("Name of the source class containing the method")] string sourceClass,
        [Description("Names of the methods to move (required)")] string[] methodNames,
        [Description("Name of the target class")] string targetClass,
        [Description("Path to the target file (optional, will create if doesn't exist or unspecified)")] string? targetFilePath = null,
        [Description("Dependencies to inject via the constructor")] string[] constructorInjections = null!,
        [Description("Dependencies to keep as parameters")] string[] parameterInjections = null!,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            filePath = Path.GetFullPath(filePath);
            if (targetFilePath != null)
                targetFilePath = Path.GetFullPath(targetFilePath);

            var methodList = methodNames;
            if (methodList.Length == 0)
                throw new McpException("Error: No method names provided");

            foreach (var m in methodList)
                EnsureNotAlreadyMoved(filePath, m);

            var solution = await ExxerFactoringHelpers.GetOrLoadSolution(solutionPath, cancellationToken);
            var document = ExxerFactoringHelpers.GetDocumentByPath(solution, filePath);

            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName == null)
                throw new McpException($"Error: Could not determine directory for file {filePath}");
            var duplicateDoc = await ExxerFactoringHelpers.FindClassInSolution(
                solution,
                targetClass,
                filePath,
                targetFilePath ?? Path.Combine(directoryName, $"{targetClass}.cs"));
            if (duplicateDoc != null)
                throw new McpException($"Error: Class {targetClass} already exists in {duplicateDoc.FilePath}");

            if (document == null && !File.Exists(Path.GetFullPath(filePath)))
                throw new McpException($"Error: File {filePath} not found");

            SyntaxNode? rootNode = document != null
                ? await document.GetSyntaxRootAsync(cancellationToken)
                : (await CSharpSyntaxTree.ParseText((await ExxerFactoringHelpers.ReadFileWithEncodingAsync(filePath, cancellationToken)).Item1).GetRootAsync(cancellationToken));
            if (rootNode == null)
                throw new McpException($"Error: Could not get syntax root for file {filePath}");
            var sourceClassNode = rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.ValueText == sourceClass);
            if (sourceClassNode == null)
                throw new McpException($"Error: Source class '{sourceClass}' not found");

            var visitor = new MethodAndMemberVisitor();
            visitor.Visit(sourceClassNode);

            var accessMemberName = MoveMethodAst.GenerateAccessMemberName(visitor.Members.Keys, targetClass);
            var accessMemberType = visitor.Members.TryGetValue(accessMemberName, out var info)
                ? info.Type
                : "field";

            string message;
            if (document != null)
            {
                var (msg, _) = await MoveInstanceMethodWithSolution(
                    document,
                    sourceClass,
                    methodList,
                    constructorInjections,
                    parameterInjections,
                    targetClass,
                    accessMemberName,
                    accessMemberType,
                    targetFilePath,
                    progress,
                    cancellationToken);
                message = msg;
            }
            else
            {
                // For single-file operations, use the bulk move method for better efficiency
                if (methodList.Length == 1)
                {
                    message = await MoveMethodFileService.MoveInstanceMethodInFile(
                        filePath,
                        sourceClass,
                        methodList[0],
                        constructorInjections,
                        parameterInjections,
                        targetClass,
                        accessMemberName,
                        accessMemberType,
                        targetFilePath,
                        progress,
                        cancellationToken);
                }
                else
                {
                    message = await MoveBulkInstanceMethodsInFile(
                        filePath,
                        sourceClass,
                        methodList,
                        targetClass,
                        accessMemberName,
                        accessMemberType,
                        targetFilePath,
                        constructorInjections,
                        parameterInjections,
                        progress,
                        cancellationToken);
                }
            }

            foreach (var m in methodList)
                MarkMoved(filePath, m);

            return message;
        }
        catch (Exception ex)
        {
            throw new McpException($"Error moving instance method: {ex.Message}", ex);
        }
    }

    private static async Task<string> MoveBulkInstanceMethodsInFile(
        string filePath,
        string sourceClass,
        string[] methodNames,
        string targetClass,
        string accessMemberName,
        string accessMemberType,
        string? targetFilePath,
        string[] constructorInjections,
        string[] parameterInjections,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(Path.GetFullPath(filePath)))
            throw new McpException($"Error: File {filePath} not found (current dir: {Directory.GetCurrentDirectory()})");

        var targetPath = targetFilePath ?? filePath;
        var sameFile = targetPath == filePath;

        var (sourceText, sourceEncoding) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(filePath, cancellationToken);

        if (sameFile)
        {
            // Same file operation - use multiple individual AST transformations
            var tree = CSharpSyntaxTree.ParseText(sourceText);
            var root = await tree.GetRootAsync(cancellationToken);

            var suggestStatic = false;
            foreach (var methodName in methodNames)
            {
                var moveResult = MoveMethodAst.MoveInstanceMethodAst(
                    root,
                    sourceClass,
                    methodName,
                    targetClass,
                    accessMemberName,
                    accessMemberType,
                    parameterInjections);
                suggestStatic |= moveResult.MovedMethod.Modifiers.Any(SyntaxKind.StaticKeyword);
                root = MoveMethodAst.AddMethodToTargetClass(moveResult.NewSourceRoot, targetClass, moveResult.MovedMethod, moveResult.Namespace);
            }

            var formatted = Formatter.Format(root, ExxerFactoringHelpers.SharedWorkspace);
            await File.WriteAllTextAsync(filePath, formatted.ToFullString(), sourceEncoding, cancellationToken);
            progress?.Report(filePath);
            var staticHint = suggestStatic ? " At least one moved method had no instance dependencies and was made static." : string.Empty;
            return $"Successfully moved {methodNames.Length} methods from {sourceClass} to {targetClass} in {filePath}. Delegate methods remain in the original class to preserve the interface.{staticHint}";
        }
        else
        {
            // Cross-file operation - update both files in memory and write once
            var sourceTree = CSharpSyntaxTree.ParseText(sourceText);
            var sourceRoot = await sourceTree.GetRootAsync(cancellationToken);

            var targetRoot = await MoveMethodFileService.LoadOrCreateTargetRoot(targetPath, cancellationToken);
            var nsName = sourceRoot.DescendantNodes()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .FirstOrDefault()?.Name.ToString();
            targetRoot = MoveMethodAst.PropagateUsings(sourceRoot, targetRoot, nsName);

            var suggestStatic2 = false;
            foreach (var methodName in methodNames)
            {
                var moveResult = MoveMethodAst.MoveInstanceMethodAst(
                    sourceRoot,
                    sourceClass,
                    methodName,
                    targetClass,
                    accessMemberName,
                    accessMemberType,
                    parameterInjections);
                suggestStatic2 |= moveResult.MovedMethod.Modifiers.Any(SyntaxKind.StaticKeyword);
                sourceRoot = moveResult.NewSourceRoot;
                targetRoot = MoveMethodAst.AddMethodToTargetClass(targetRoot, targetClass, moveResult.MovedMethod, moveResult.Namespace);
            }

            var formattedSource = Formatter.Format(sourceRoot, ExxerFactoringHelpers.SharedWorkspace);
            await File.WriteAllTextAsync(filePath, formattedSource.ToFullString(), sourceEncoding, cancellationToken);
            progress?.Report(filePath);

            var formattedTarget = Formatter.Format(targetRoot, ExxerFactoringHelpers.SharedWorkspace);
            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (targetDirectory != null)
                Directory.CreateDirectory(targetDirectory);
            var targetEncoding = File.Exists(targetPath)
                ? await ExxerFactoringHelpers.GetFileEncodingAsync(targetPath, cancellationToken)
                : sourceEncoding;
            await File.WriteAllTextAsync(targetPath, formattedTarget.ToFullString(), targetEncoding, cancellationToken);
            progress?.Report(targetPath);

            var staticHint2 = suggestStatic2 ? " At least one moved method had no instance dependencies and was made static." : string.Empty;
            return $"Successfully moved {methodNames.Length} methods from {sourceClass} to {targetClass} in {targetPath}. Delegate methods remain in the original class to preserve the interface.{staticHint2}";
        }
    }

    public static async Task<(string, Document)> MoveInstanceMethodWithSolution(
        Document document,
        string sourceClassName,
        string[] methodNames,
        string[] constructorInjections,
        string[] parameterInjections,
        string targetClassName,
        string accessMemberName,
        string accessMemberType,
        string? targetFilePath,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var messages = new List<string>();
        var currentDocument = document;

        foreach (var methodName in methodNames)
        {
            var targetPath = targetFilePath ?? currentDocument.FilePath ?? throw new InvalidOperationException("Document has no file path");
            var sameFile = targetPath == (currentDocument.FilePath ?? throw new InvalidOperationException("Document has no file path"));

            var message = await MoveMethodFileService.MoveInstanceMethodInFile(
                currentDocument.FilePath ?? throw new InvalidOperationException("Document has no file path"),
                sourceClassName,
                methodName,
                constructorInjections,
                parameterInjections,
                targetClassName,
                accessMemberName,
                accessMemberType,
                targetFilePath,
                progress,
                cancellationToken);

            if (sameFile)
            {
                var (newText, _) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(targetPath, cancellationToken);
                var newRoot = await CSharpSyntaxTree.ParseText(newText).GetRootAsync(cancellationToken);
                var newDocument = document.Project.Solution.WithDocumentSyntaxRoot(currentDocument.Id, newRoot).GetDocument(currentDocument.Id);
                if (newDocument == null)
                    throw new InvalidOperationException("Could not get updated document");
                currentDocument = newDocument;
            }
            else
            {
                var (newSourceText, _) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(currentDocument.FilePath ?? throw new InvalidOperationException("Document has no file path"), cancellationToken);
                var newSourceRoot = await CSharpSyntaxTree.ParseText(newSourceText).GetRootAsync(cancellationToken);
                var solution = document.Project.Solution.WithDocumentSyntaxRoot(currentDocument.Id, newSourceRoot);

                var project = solution.GetProject(document.Project.Id);
                if (project is null)
                {
                    continue;
                }
                var targetDocument = project.Documents.FirstOrDefault(d => d.FilePath == targetPath);
                if (targetDocument == null)
                {
                    var (targetText, targetEnc) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(targetPath, cancellationToken);
                    var targetSourceText = SourceText.From(targetText, targetEnc);
                    targetDocument = project.AddDocument(Path.GetFileName(targetPath), targetSourceText, filePath: targetPath);
                    solution = targetDocument.Project.Solution;
                }
                else
                {
                    var (targetText, targetEnc) = await ExxerFactoringHelpers.ReadFileWithEncodingAsync(targetPath, cancellationToken);
                    var targetSourceText = SourceText.From(targetText, targetEnc);
                    solution = solution.WithDocumentText(targetDocument.Id, targetSourceText);
                }
                var newDocument = solution.GetDocument(currentDocument.Id);
                if (newDocument == null)
                    throw new InvalidOperationException("Could not get updated document");
                currentDocument = newDocument;
            }

            ExxerFactoringHelpers.UpdateSolutionCache(currentDocument);
            messages.Add(message);
        }

        return (string.Join("\n", messages), currentDocument);
    }
}