namespace ExxerFactor.Mcp.Tests.Roslyn.Walkers;

public partial class RoslynTransformationTests
{
    [Fact]
    public void AccessMemberTypeWalker_DetectsFieldAndProperty()
    {
        const string code = "class C { int f; string P { get; } }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var fieldWalker = new AccessMemberTypeWalker("f");
        fieldWalker.Visit(root);
        Assert.Equal("field", fieldWalker.MemberType);

        var propWalker = new AccessMemberTypeWalker("P");
        propWalker.Visit(root);
        Assert.Equal("property", propWalker.MemberType);
    }
}