using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorMCP.Core.SyntaxRewriters;

public class BodyOmitter : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitBlock(BlockSyntax node)
    {
        return SyntaxFactory.Block(SyntaxFactory.ParseStatement("// ...\n"));
    }

    public override SyntaxNode? VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
    {
        return SyntaxFactory.Block(SyntaxFactory.ParseStatement("// ...\n"));
    }
}