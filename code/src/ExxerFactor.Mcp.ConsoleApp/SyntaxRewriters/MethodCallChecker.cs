namespace ExxerFactor.Mcp.App.SyntaxRewriters;

internal class MethodCallChecker : TrackedNameWalker
{
    public bool HasMethodCalls => Matches.Count > 0;

    public MethodCallChecker(HashSet<string> classMethodNames)
        : base(classMethodNames)
    {
    }

    protected override bool TryRecordInvocation(InvocationExpressionSyntax node)
    {
        if (node.Expression is IdentifierNameSyntax identifier && IsTarget(identifier.Identifier.ValueText))
        {
            RecordMatch(identifier.Identifier.ValueText);
            return true;
        }
        return false;
    }
}