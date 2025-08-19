namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void IdentifierRenameRewriter_RenamesField()
    {
        var code = "class C{ int x; int M(){ return x; } }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("test", new[] { tree });
        var model = compilation.GetSemanticModel(tree);
        var root = tree.GetRoot();
        var field = root.DescendantNodes().OfType<VariableDeclaratorSyntax>().First();
        var symbol = model.GetDeclaredSymbol(field)!;
        var map = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default)
        {
            [symbol] = "p"
        };
        var rewriter = new IdentifierRenameRewriter(model, map, null);
        var newRoot = rewriter.Visit(root)!;
        Assert.Contains("p", newRoot.ToFullString());
    }
}