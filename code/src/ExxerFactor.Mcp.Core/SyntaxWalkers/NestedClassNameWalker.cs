using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxWalkers;

public class NestedClassNameWalker : NameCollectorWalker
{
    private readonly ClassDeclarationSyntax _origin;

    public NestedClassNameWalker(ClassDeclarationSyntax origin)
    {
        _origin = origin;
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node.Parent == _origin)
            Add(node.Identifier.ValueText);
        base.VisitClassDeclaration(node);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        if (node.Parent == _origin)
            Add(node.Identifier.ValueText);
        base.VisitEnumDeclaration(node);
    }
}