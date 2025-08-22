namespace ExxerFactor.Mcp.App.SyntaxRewriters;

internal class InstanceMemberUsageChecker : TrackedNameWalker
{
    public bool HasInstanceMemberUsage => Matches.Count > 0;

    public InstanceMemberUsageChecker(HashSet<string> knownInstanceMembers)
        : base(knownInstanceMembers)
    {
    }
}