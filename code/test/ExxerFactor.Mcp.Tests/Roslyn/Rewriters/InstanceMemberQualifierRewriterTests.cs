namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void InstanceMemberQualifierRewriter_QualifiesMember()
    {
        var method = SyntaxFactory.ParseMemberDeclaration("void M(){ Value = 1; }") as MethodDeclarationSyntax;
        var rewriter = new InstanceMemberQualifierRewriter("inst", knownMembers: new HashSet<string> { "Value" });
        var result = rewriter.Visit(method!)!.NormalizeWhitespace().ToFullString();
        Assert.Contains("inst.Value", result);
    }
}