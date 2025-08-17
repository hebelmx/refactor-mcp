namespace RefactorMCP.Core.SyntaxWalkers;

public class PrivateFieldUsageWalker : TrackedNameWalker
{
    public HashSet<string> UsedFields => Matches;

    public PrivateFieldUsageWalker(HashSet<string> privateFieldNames)
        : base(privateFieldNames)
    {
    }
}