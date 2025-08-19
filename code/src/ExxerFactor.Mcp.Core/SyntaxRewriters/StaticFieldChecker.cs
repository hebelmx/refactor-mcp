using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExxerFactor.Mcp.Core.SyntaxWalkers;

namespace ExxerFactor.Mcp.Core.SyntaxRewriters;

public class StaticFieldChecker : TrackedNameWalker
{
    public bool HasStaticFieldReferences => Matches.Count > 0;

    public StaticFieldChecker(HashSet<string> staticFieldNames)
        : base(staticFieldNames)
    {
    }

    protected override bool ShouldRecordIdentifier(IdentifierNameSyntax node)
    {
        var parent = node.Parent;
        if (!IsTarget(node.Identifier.ValueText) || IsParameterOrType(parent))
            return false;

        if (parent is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name == node;
        }

        return true;
    }
}