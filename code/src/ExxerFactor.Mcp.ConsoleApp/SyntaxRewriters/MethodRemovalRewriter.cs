namespace ExxerFactor.Mcp.App.SyntaxRewriters;

internal class MethodRemovalRewriter : DeclarationRemovalRewriter<MethodDeclarationSyntax>
{
    public MethodRemovalRewriter(string methodName)
        : base(methodName)
    {
    }

    protected override bool IsTarget(MethodDeclarationSyntax node)
        => node.Identifier.ValueText == Name;
}