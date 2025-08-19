using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExxerFactor.Mcp.Core.SyntaxWalkers;

public class ClassCollectorWalker : TypeCollectorWalker<ClassDeclarationSyntax>
{
    public Dictionary<string, ClassDeclarationSyntax> Classes => Types;
}