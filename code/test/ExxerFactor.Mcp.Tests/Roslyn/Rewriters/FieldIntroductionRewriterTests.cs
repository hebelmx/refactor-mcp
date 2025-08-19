namespace ExxerFactor.Mcp.Tests.Roslyn.Rewriters;

public partial class RoslynTransformationTests
{
    [Fact]
    public void FieldIntroductionRewriter_AddsFieldAndReplacesExpression()
    {
        var tree = CSharpSyntaxTree.ParseText("class C{int Get(){return 1;}} ");
        var root = tree.GetRoot();
        var expr = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
        var fieldRef = SyntaxFactory.IdentifierName("value");
        var fieldDecl = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("int"))
                    .AddVariables(SyntaxFactory.VariableDeclarator("value")
                        .WithInitializer(SyntaxFactory.EqualsValueClause(expr))))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
        var classNode = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var rewriter = new FieldIntroductionRewriter(expr, fieldRef, fieldDecl, classNode);
        var newRoot = Formatter.Format(rewriter.Visit(root)!, new AdhocWorkspace());
        var text = newRoot.ToFullString();
        Assert.Contains("private int value", text);
        Assert.Contains("return value", text);
    }
}