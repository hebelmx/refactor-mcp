using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxRewriters;

public class MethodRemovalRewriter : DeclarationRemovalRewriter<MethodDeclarationSyntax>
{
    public MethodRemovalRewriter(string methodName)
        : base(methodName)
    {
    }

    protected override bool IsTarget(MethodDeclarationSyntax node)
        => node.Identifier.ValueText == Name;
}

