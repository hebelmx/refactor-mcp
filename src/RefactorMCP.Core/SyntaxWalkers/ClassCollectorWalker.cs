using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorMCP.Core.SyntaxWalkers
{
    public class ClassCollectorWalker : TypeCollectorWalker<ClassDeclarationSyntax>
    {
        public Dictionary<string, ClassDeclarationSyntax> Classes => Types;
    }
}
