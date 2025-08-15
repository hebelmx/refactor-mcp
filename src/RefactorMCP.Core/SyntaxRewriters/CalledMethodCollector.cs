using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorMCP.Core.SyntaxWalkers;

namespace RefactorMCP.Core.SyntaxRewriters;

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