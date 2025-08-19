namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void SetterToInitRewriter_ReplacesSetterWithInit()
    {
        var prop = SyntaxFactory.ParseMemberDeclaration("public int P { get; set; }") as PropertyDeclarationSyntax;
        var rewriter = new SetterToInitRewriter("P");
        var result = rewriter.Visit(prop!)!.NormalizeWhitespace().ToFullString();
        Assert.Contains("init", result);
    }
}