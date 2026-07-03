using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class ParserTests
{
    [Fact]
    public void MultiplicationBindsTighterThanAddition()
    {
        // 2 + 3 * 4  =>  (+ 2 (* 3 4))
        var expr = Fc.FirstDeclInit("ingredient a = 2 + 3 * 4");
        Assert.Equal(NodeType.BinaryExpression, expr.Type);
        Assert.Equal(Syntax.Addition, expr.Value);
        Assert.Equal(NodeType.NumberLiteral, expr.Children[0].Type);
        Assert.Equal(NodeType.BinaryExpression, expr.Children[1].Type);
        Assert.Equal(Syntax.Multiplication, expr.Children[1].Value);
    }

    [Fact]
    public void SubtractionIsLeftAssociative()
    {
        // 10 - 2 - 3  =>  (- (- 10 2) 3), not (- 10 (- 2 3))
        var expr = Fc.FirstDeclInit("ingredient a = 10 - 2 - 3");
        Assert.Equal(NodeType.BinaryExpression, expr.Type);
        Assert.Equal(Syntax.Subtraction, expr.Value);

        var left = expr.Children[0];
        Assert.Equal(NodeType.BinaryExpression, left.Type);
        Assert.Equal(Syntax.Subtraction, left.Value);
        Assert.Equal("10", left.Children[0].Value);
        Assert.Equal("2", left.Children[1].Value);
        Assert.Equal("3", expr.Children[1].Value);
    }

    [Fact]
    public void ComparisonBindsLooserThanArithmetic()
    {
        // x + 1 < 5  =>  (< (+ x 1) 5)
        var expr = Fc.FirstDeclInit("ingredient a = x + 1 < 5");
        Assert.Equal(NodeType.BinaryExpression, expr.Type);
        Assert.Equal(Syntax.LessThan, expr.Value);
        Assert.Equal(NodeType.BinaryExpression, expr.Children[0].Type);
        Assert.Equal(Syntax.Addition, expr.Children[0].Value);
    }

    [Fact]
    public void ParenthesesOverridePrecedence()
    {
        // (2 + 3) * 4  =>  (* (+ 2 3) 4)
        var expr = Fc.FirstDeclInit("ingredient a = (2 + 3) * 4");
        Assert.Equal(Syntax.Multiplication, expr.Value);
        Assert.Equal(NodeType.BinaryExpression, expr.Children[0].Type);
        Assert.Equal(Syntax.Addition, expr.Children[0].Value);
    }
}
