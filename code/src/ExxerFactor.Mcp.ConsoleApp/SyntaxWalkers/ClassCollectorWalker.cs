namespace ExxerFactor.Mcp.App.SyntaxWalkers;

internal class ClassCollectorWalker : TypeCollectorWalker<ClassDeclarationSyntax>
{
    public Dictionary<string, ClassDeclarationSyntax> Classes => Types;
}