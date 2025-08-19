namespace ExxerFactor.Mcp.App.SyntaxWalkers;

internal abstract class NameCollectorWalker : CSharpSyntaxWalker
{
    public HashSet<string> Names { get; } = new();

    protected void Add(string name) => Names.Add(name);
}