using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxWalkers;

public class MethodDependencyWalker : CSharpSyntaxWalker
{
    private readonly HashSet<string> _candidateMethods;
    public HashSet<string> Dependencies { get; } = new();

    public MethodDependencyWalker(HashSet<string> candidateMethods)
    {
        _candidateMethods = candidateMethods;
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var identifier = node.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.ValueText,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.ValueText,
            _ => null
        };

        if (identifier != null && _candidateMethods.Contains(identifier))
        {
            Dependencies.Add(identifier);
        }

        base.VisitInvocationExpression(node);
    }
}