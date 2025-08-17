using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorMCP.Core.SyntaxWalkers;

public class InterfaceCollectorWalker : TypeCollectorWalker<InterfaceDeclarationSyntax>
{
    public Dictionary<string, InterfaceDeclarationSyntax> Interfaces => Types;
}