namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void StaticFieldRewriter_QualifiesStaticField()
    {
        var method = SyntaxFactory.ParseMemberDeclaration("void Test(){ x = 1; }") as MethodDeclarationSyntax;
        var rewriter = new StaticFieldRewriter(new HashSet<string> { "x" }, "C");
        var result = rewriter.Visit(method!)!.NormalizeWhitespace().ToFullString();
        Assert.Contains("C.x", result);
    }
}