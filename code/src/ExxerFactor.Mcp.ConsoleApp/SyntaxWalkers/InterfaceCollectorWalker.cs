namespace ExxerFactor.Mcp.App.SyntaxWalkers;

internal class InterfaceCollectorWalker : TypeCollectorWalker<InterfaceDeclarationSyntax>
{
    public Dictionary<string, InterfaceDeclarationSyntax> Interfaces => Types;
}