using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxRewriters;

public static class InvocationHelpers
{
    public static string? GetInvokedMethodName(InvocationExpressionSyntax node)
        => node.Expression switch
        {
            IdentifierNameSyntax id => id.Identifier.ValueText,
            MemberAccessExpressionSyntax ma when ma.Name is IdentifierNameSyntax id => id.Identifier.ValueText,
            _ => null
        };

    public static bool IsInvocationOf(InvocationExpressionSyntax node, string methodName)
        => GetInvokedMethodName(node) == methodName;

    public static bool IsBaseInvocationOf(InvocationExpressionSyntax node, string methodName)
        => node.Expression is MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax, Name: IdentifierNameSyntax id } && id.Identifier.ValueText == methodName;
}