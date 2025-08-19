namespace ExxerFactor.Mcp.Core.Tests.SyntaxRewriters;

public class ConstructorInjectionRewriterTests
{
    [Fact]
    public void VisitMethodDeclaration_WithTargetMethod_ShouldRemoveParameter()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public void TestMethod(string param1, int param2, bool param3)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var methodDecl = result.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        methodDecl.ParameterList.Parameters.Count().ShouldBe(2);
        methodDecl.ParameterList.Parameters.ShouldNotContain(p => p.Identifier.Text == "param2");
        methodDecl.ParameterList.Parameters.ShouldContain(p => p.Identifier.Text == "param1");
        methodDecl.ParameterList.Parameters.ShouldContain(p => p.Identifier.Text == "param3");
    }

    [Fact]
    public void VisitMethodDeclaration_WithNonTargetMethod_ShouldNotModify()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }

    public void OtherMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var otherMethod = result.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == "OtherMethod");
        otherMethod.ParameterList.Parameters.Count().ShouldBe(2);
        otherMethod.ParameterList.Parameters.ShouldContain(p => p.Identifier.Text == "param2");
    }

    [Fact]
    public void VisitIdentifierName_WithTargetParameter_ShouldReplaceWithField()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
        Console.WriteLine(param2);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var identifierNames = result.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();
        identifierNames.ShouldContain(i => i.Identifier.Text == "_param2");
        identifierNames.ShouldNotContain(i => i.Identifier.Text == "param2");
    }

    [Fact]
    public void VisitIdentifierName_WithNonTargetParameter_ShouldNotReplace()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
        Console.WriteLine(param2);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var identifierNames = result.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();
        identifierNames.ShouldContain(i => i.Identifier.Text == "param1");
        identifierNames.ShouldContain(i => i.Identifier.Text == "_param2");
    }

    [Fact]
    public void VisitInvocationExpression_WithTargetMethodCall_ShouldRemoveArgument()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public void TestMethod(string param1, int param2, bool param3)
    {
        Console.WriteLine(param1);
    }

    public void CallTestMethod()
    {
        TestMethod(""hello"", 42, true);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var invocation = result.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .First(i => i.Expression.ToString().Contains("TestMethod"));
        invocation.ArgumentList.Arguments.Count().ShouldBe(2);
        invocation.ArgumentList.Arguments.ShouldNotContain(a => a.ToString().Contains("42"));
    }

    [Fact]
    public void VisitConstructorDeclaration_WithNoExistingParameter_ShouldAddParameterAndField()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public TestClass()
    {
        // Empty constructor
    }

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var constructor = result.DescendantNodes().OfType<ConstructorDeclarationSyntax>().First();
        constructor.ParameterList.Parameters.Count().ShouldBe(1);
        constructor.ParameterList.Parameters.ShouldContain(p => p.Identifier.Text == "param2");

        var classDecl = result.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        classDecl.Members.OfType<FieldDeclarationSyntax>().Count().ShouldBe(1);
        classDecl.Members.OfType<FieldDeclarationSyntax>().First()
            .Declaration.Variables.ShouldContain(v => v.Identifier.Text == "_param2");
    }

    [Fact]
    public void VisitConstructorDeclaration_WithExistingParameter_ShouldNotAddDuplicate()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public TestClass(int param2)
    {
        // Constructor with parameter
    }

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var constructor = result.DescendantNodes().OfType<ConstructorDeclarationSyntax>().First();
        constructor.ParameterList.Parameters.Count().ShouldBe(1);
        constructor.ParameterList.Parameters.ShouldContain(p => p.Identifier.Text == "param2");
    }

    [Fact]
    public void VisitClassDeclaration_WithPropertyInjection_ShouldAddProperty()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public TestClass()
    {
        // Empty constructor
    }

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", true); // useProperty = true

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var classDecl = result.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        classDecl.Members.OfType<PropertyDeclarationSyntax>().Count().ShouldBe(1);
        classDecl.Members.OfType<PropertyDeclarationSyntax>().First()
            .Identifier.Text.ShouldBe("_param2");
        classDecl.Members.OfType<FieldDeclarationSyntax>().ShouldBeEmpty();
    }

    [Fact]
    public void VisitClassDeclaration_WithFieldInjection_ShouldAddField()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public TestClass()
    {
        // Empty constructor
    }

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false); // useProperty = false

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var classDecl = result.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        classDecl.Members.OfType<FieldDeclarationSyntax>().Count().ShouldBe(1);
        classDecl.Members.OfType<FieldDeclarationSyntax>().First()
            .Declaration.Variables.ShouldContain(v => v.Identifier.Text == "_param2");
        classDecl.Members.OfType<PropertyDeclarationSyntax>().ShouldBeEmpty();
    }

    [Fact]
    public void VisitClassDeclaration_WithExistingField_ShouldNotAddDuplicate()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    private int _param2;

    public TestClass()
    {
        // Empty constructor
    }

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var classDecl = result.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        classDecl.Members.OfType<FieldDeclarationSyntax>().Count().ShouldBe(1);
    }

    [Fact]
    public void VisitClassDeclaration_WithExistingProperty_ShouldNotAddDuplicate()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public int _param2 { get; set; }

    public TestClass()
    {
        // Empty constructor
    }

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", true);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var classDecl = result.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        classDecl.Members.OfType<PropertyDeclarationSyntax>().Count().ShouldBe(1);
    }

    [Fact]
    public void VisitConstructorDeclaration_ShouldAddAssignmentStatement()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public TestClass()
    {
        // Empty constructor
    }

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var constructor = result.DescendantNodes().OfType<ConstructorDeclarationSyntax>().First();
        constructor.Body!.Statements.Count().ShouldBe(1);
        constructor.Body.Statements.First().ShouldBeOfType<ExpressionStatementSyntax>();

        var assignment = constructor.Body.Statements.First() as ExpressionStatementSyntax;
        assignment!.Expression.ShouldBeOfType<AssignmentExpressionSyntax>();

        var assignmentExpr = assignment.Expression as AssignmentExpressionSyntax;
        assignmentExpr!.Left.ToString().ShouldBe("_param2");
        assignmentExpr.Right.ToString().ShouldBe("param2");
    }

    [Fact]
    public void VisitConstructorDeclaration_WithExistingBody_ShouldAddToExistingBody()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public TestClass()
    {
        var x = 1;
        Console.WriteLine(x);
    }

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var constructor = result.DescendantNodes().OfType<ConstructorDeclarationSyntax>().First();
        constructor.Body!.Statements.Count().ShouldBe(3); // Original 2 + new assignment
        constructor.Body.Statements.ShouldContain(s => s.ToString().Contains("_param2 = param2"));
    }

    [Fact]
    public void VisitConstructorDeclaration_WithNoBody_ShouldCreateBody()
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public TestClass();

    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", 1,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        var constructor = result.DescendantNodes().OfType<ConstructorDeclarationSyntax>().First();
        constructor.Body.ShouldNotBeNull();
        constructor.Body!.Statements.Count().ShouldBe(1);
        constructor.Body.Statements.First().ToString().ShouldContain("_param2 = param2");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(10)]
    public void VisitMethodDeclaration_WithInvalidParameterIndex_ShouldHandleGracefully(int parameterIndex)
    {
        // Arrange
        var sourceCode = @"
public class TestClass
{
    public void TestMethod(string param1, int param2)
    {
        Console.WriteLine(param1);
    }
}";
        var rewriter = new ConstructorInjectionRewriter("TestMethod", "param2", parameterIndex,
            SyntaxFactory.ParseTypeName("int"), "_param2", false);

        // Act
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        var result = rewriter.Visit(root);

        // Assert
        // Should not throw exception, but may not modify as expected
        var methodDecl = result.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        methodDecl.ShouldNotBeNull();
    }
}