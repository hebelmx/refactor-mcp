using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxRewriters;

public class FieldRemovalRewriter : DeclarationRemovalRewriter<FieldDeclarationSyntax>
{
    public FieldRemovalRewriter(string fieldName)
        : base(fieldName)
    {
    }

    protected override bool IsTarget(FieldDeclarationSyntax node)
        => node.Declaration.Variables.Any(v => v.Identifier.ValueText == Name);

    protected override SeparatedSyntaxList<VariableDeclaratorSyntax>? GetDeclarators(FieldDeclarationSyntax node)
        => node.Declaration.Variables;

    protected override FieldDeclarationSyntax WithDeclarators(FieldDeclarationSyntax node, SeparatedSyntaxList<VariableDeclaratorSyntax> declarators)
        => node.WithDeclaration(node.Declaration.WithVariables(declarators));
}

