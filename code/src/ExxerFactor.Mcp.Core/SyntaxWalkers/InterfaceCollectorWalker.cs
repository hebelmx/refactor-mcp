using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxWalkers;

public class InterfaceCollectorWalker : TypeCollectorWalker<InterfaceDeclarationSyntax>
{
    public Dictionary<string, InterfaceDeclarationSyntax> Interfaces => Types;
}