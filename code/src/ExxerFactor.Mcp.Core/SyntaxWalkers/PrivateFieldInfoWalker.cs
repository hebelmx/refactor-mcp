using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxWalkers;

public class PrivateFieldInfoWalker : CSharpSyntaxWalker
{
    public Dictionary<string, TypeSyntax> Infos { get; } = new();

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            foreach (var variable in node.Declaration.Variables)
                Infos[variable.Identifier.ValueText] = node.Declaration.Type;
        }
        base.VisitFieldDeclaration(node);
    }
}