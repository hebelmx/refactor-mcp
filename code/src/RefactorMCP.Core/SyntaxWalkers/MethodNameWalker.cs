using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorMCP.Core.SyntaxWalkers;

public class MethodNameWalker : NameCollectorWalker
{
    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        Add(node.Identifier.ValueText);
        base.VisitMethodDeclaration(node);
    }
}