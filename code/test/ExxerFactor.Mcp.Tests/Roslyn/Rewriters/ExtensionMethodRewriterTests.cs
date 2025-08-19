namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void ExtensionMethodRewriter_AddsThisParameterAndQualifiesMembers()
    {
        var method = SyntaxFactory.ParseMemberDeclaration("void Print(){ Console.WriteLine(Value); }") as MethodDeclarationSyntax;
        var rewriter = new ExtensionMethodRewriter("inst", "C", new HashSet<string> { "Value" });
        var result = rewriter.Rewrite(method!).NormalizeWhitespace().ToFullString();
        Assert.Contains("static", result);
        Assert.Contains("this C inst", result);
        Assert.Contains("inst.Value", result);
    }
}