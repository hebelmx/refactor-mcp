namespace ExxerFactor.Mcp.App.SyntaxWalkers;

internal class MethodStaticWalker : CSharpSyntaxWalker
{
    private readonly HashSet<string> _methodNames;
    public Dictionary<string, bool> IsStaticMap { get; } = new();

    public MethodStaticWalker(IEnumerable<string> methodNames)
    {
        _methodNames = new HashSet<string>(methodNames);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var name = node.Identifier.ValueText;
        if (_methodNames.Contains(name))
            IsStaticMap[name] = node.Modifiers.Any(SyntaxKind.StaticKeyword);
        base.VisitMethodDeclaration(node);
    }
}