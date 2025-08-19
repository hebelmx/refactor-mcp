namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void MethodReferenceRewriter_QualifiesUnqualifiedReference()
    {
        var method = SyntaxFactory.ParseMemberDeclaration("void Test(){ obj.Evt += OnEvt; }") as MethodDeclarationSyntax;
        var rewriter = new MethodReferenceRewriter(new HashSet<string> { "OnEvt" }, "inst");
        var result = rewriter.Visit(method!)!.NormalizeWhitespace().ToFullString();
        Assert.Contains("obj.Evt += inst.OnEvt", result);
    }

    [Fact]
    public void MethodReferenceRewriter_QualifiesThisReference()
    {
        var method = SyntaxFactory.ParseMemberDeclaration("void Test(){ obj.Evt += this.OnEvt; }") as MethodDeclarationSyntax;
        var rewriter = new MethodReferenceRewriter(new HashSet<string> { "OnEvt" }, "inst");
        var result = rewriter.Visit(method!)!.NormalizeWhitespace().ToFullString();
        Assert.Contains("obj.Evt += inst.OnEvt", result);
        Assert.DoesNotContain("this.OnEvt", result);
    }
}