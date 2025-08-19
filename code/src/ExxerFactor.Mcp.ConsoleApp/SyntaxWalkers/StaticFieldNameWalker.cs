namespace ExxerFactor.Mcp.App.SyntaxWalkers;

internal class StaticFieldNameWalker : NameCollectorWalker
{
    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            foreach (var variable in node.Declaration.Variables)
                Add(variable.Identifier.ValueText);
        }
        base.VisitFieldDeclaration(node);
    }
}