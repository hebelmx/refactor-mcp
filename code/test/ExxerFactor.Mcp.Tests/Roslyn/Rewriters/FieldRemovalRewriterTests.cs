namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void FieldRemovalRewriter_RemovesField()
    {
        var tree = CSharpSyntaxTree.ParseText("class A{int x;}");
        var root = tree.GetRoot();
        var rewriter = new FieldRemovalRewriter("x");
        var newRoot = Formatter.Format(rewriter.Visit(root)!, new AdhocWorkspace());
        Assert.Equal("class A { }", newRoot.ToFullString().Trim());
    }
}