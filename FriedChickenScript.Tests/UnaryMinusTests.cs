using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class UnaryMinusTests
{
    [Theory]
    [InlineData("orderUp(-5)", "-5")]
    [InlineData("orderUp(-3.5)", "-3.5")]
    [InlineData("orderUp(-5 + 3)", "-2")]        // unary binds tighter than binary +
    [InlineData("orderUp(5 - -3)", "8")]         // binary minus then unary minus
    [InlineData("orderUp(-5 * 2)", "-10")]
    [InlineData("orderUp(-(2 + 3))", "-5")]      // negate a parenthesised expression
    [InlineData("orderUp(- -5)", "5")]           // double negation
    public void NegatesNumbers(string source, string expected)
    {
        Assert.Equal(expected, Fc.Run(source));
    }

    [Fact]
    public void NegatesAVariable()
    {
        Assert.Equal("-10", Fc.Run(@"
            ingredient x = 10
            orderUp(-x)"));
    }

    [Fact]
    public void NegatingANonNumberThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp(-COOKED)"));
    }

    [Fact]
    public void BareNegativeIsEchoedByTheRepl()
    {
        var interp = new Interpreter();
        var program = new Parser(new Lexer("-5").Tokenise()).Parse();
        Assert.Equal(-5, interp.RunRepl(program));
    }
}
