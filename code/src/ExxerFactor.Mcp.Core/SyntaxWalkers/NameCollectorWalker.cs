using Microsoft.CodeAnalysis.CSharp;

namespace ExxerFactor.Mcp.Core.SyntaxWalkers;

public abstract class NameCollectorWalker : CSharpSyntaxWalker
{
    public HashSet<string> Names { get; } = new();

    protected void Add(string name) => Names.Add(name);
}