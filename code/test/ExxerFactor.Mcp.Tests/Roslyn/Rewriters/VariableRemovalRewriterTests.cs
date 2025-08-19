namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void VariableRemovalRewriter_RemovesVariable()
    {
        var tree = CSharpSyntaxTree.ParseText("void M(){int x=0;}");
        var root = tree.GetRoot();
        var varNode = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
        var rewriter = new VariableRemovalRewriter("x", varNode.Span);
        var newRoot = Formatter.Format(rewriter.Visit(root)!, new AdhocWorkspace());
        Assert.Equal("void M() { }", newRoot.ToFullString().Trim());
    }
}