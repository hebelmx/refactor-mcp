namespace ExxerFactor.Mcp.App.SyntaxRewriters;

internal class VariableRemovalRewriter : DeclarationRemovalRewriter<LocalDeclarationStatementSyntax>
{
    private readonly TextSpan _span;

    public VariableRemovalRewriter(string variableName, TextSpan span)
        : base(variableName)
    {
        _span = span;
    }

    protected override bool IsTarget(LocalDeclarationStatementSyntax node)
        => node.Declaration.Variables.Any(v => v.Identifier.ValueText == Name && v.Span.Equals(_span));

    protected override SeparatedSyntaxList<VariableDeclaratorSyntax>? GetDeclarators(LocalDeclarationStatementSyntax node)
        => node.Declaration.Variables;

    protected override LocalDeclarationStatementSyntax WithDeclarators(LocalDeclarationStatementSyntax node, SeparatedSyntaxList<VariableDeclaratorSyntax> declarators)
        => node.WithDeclaration(node.Declaration.WithVariables(declarators));
}