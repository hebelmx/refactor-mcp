using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxWalkers;

public class MethodNameWalker : NameCollectorWalker
{
    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        Add(node.Identifier.ValueText);
        base.VisitMethodDeclaration(node);
    }
}