namespace ExxerFactor.Mcp.App.SyntaxWalkers;

internal class MethodNameWalker : NameCollectorWalker
{
    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        Add(node.Identifier.ValueText);
        base.VisitMethodDeclaration(node);
    }
}