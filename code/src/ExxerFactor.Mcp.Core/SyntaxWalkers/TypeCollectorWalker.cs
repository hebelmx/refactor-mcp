using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxWalkers;

public class TypeCollectorWalker<T> : CSharpSyntaxWalker where T : TypeDeclarationSyntax
{
    public Dictionary<string, T> Types { get; } = new();

    public override void Visit(SyntaxNode? node)
    {
        if (node is T typed)
        {
            var name = typed.Identifier.ValueText;
            if (!Types.ContainsKey(name))
                Types[name] = typed;
        }
        base.Visit(node);
    }
}