namespace ExxerFactor.Mcp.Tests.ToolsNew;

public class MoveProtectedOverrideDependencyToolTests
{
    [Fact]
    public void MoveMethod_UpdatesProtectedOverrideDependency()
    {
        var source = @"public class Base { protected virtual void DoIt() {} } public class Derived : Base { protected override void DoIt() {} public void Another() { DoIt(); } } public class Target { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = tree.GetRoot();

        var moveResult = MoveMethodAst.MoveInstanceMethodAst(root, "Derived", "Another", "Target", "", "");
        var updatedRoot = MoveMethodAst.AddMethodToTargetClass(moveResult.NewSourceRoot, "Target", moveResult.MovedMethod, moveResult.Namespace);
        var formattedRoot = Formatter.Format(updatedRoot, new AdhocWorkspace());

        var derivedClass = formattedRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First(c => c.Identifier.ValueText == "Derived");
        var method = derivedClass.Members.OfType<MethodDeclarationSyntax>().First(m => m.Identifier.ValueText == "DoIt");
        var mods = method.Modifiers.ToFullString();

        Assert.Contains("protected", mods);
        Assert.Contains("internal", mods);
        Assert.Contains("override", mods);
    }
}