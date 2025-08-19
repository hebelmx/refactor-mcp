using Microsoft.CodeAnalysis.CSharp.Syntax;
using ExxerFactor.Mcp.Core.SyntaxWalkers;

namespace ExxerFactor.Mcp.Core.SyntaxRewriters;

public class CalledMethodCollector : TrackedNameWalker
{
    public HashSet<string> CalledMethods => Matches;

    public CalledMethodCollector(HashSet<string> methodNames)
        : base(methodNames)
    {
    }

    protected override bool TryRecordInvocation(InvocationExpressionSyntax node)
    {
        var name = GetInvocationName(node);
        if (name != null && IsTarget(name))
        {
            RecordMatch(name);
            return true;
        }
        return false;
    }
}