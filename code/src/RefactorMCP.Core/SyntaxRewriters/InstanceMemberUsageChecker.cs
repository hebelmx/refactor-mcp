using RefactorMCP.Core.SyntaxWalkers;

namespace RefactorMCP.Core.SyntaxRewriters;

public class InstanceMemberUsageChecker : TrackedNameWalker
{
    public bool HasInstanceMemberUsage => Matches.Count > 0;

    public InstanceMemberUsageChecker(HashSet<string> knownInstanceMembers)
        : base(knownInstanceMembers)
    {
    }
}