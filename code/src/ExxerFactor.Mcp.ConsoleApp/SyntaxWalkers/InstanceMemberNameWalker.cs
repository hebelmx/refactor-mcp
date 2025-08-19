namespace ExxerFactor.Mcp.App.SyntaxWalkers;

internal class InstanceMemberNameWalker : NameCollectorWalker
{
    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (var variable in node.Declaration.Variables)
            Add(variable.Identifier.ValueText);
        base.VisitFieldDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        Add(node.Identifier.ValueText);
        base.VisitPropertyDeclaration(node);
    }
}