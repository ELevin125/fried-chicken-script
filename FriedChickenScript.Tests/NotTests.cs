using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class NotTests
{
    [Theory]
    [InlineData("orderUp(!COOKED)", "RAW")]
    [InlineData("orderUp(!RAW)", "COOKED")]
    [InlineData("orderUp(!EMPTY)", "COOKED")]   // null is falsy, so !EMPTY is true
    [InlineData("orderUp(!0)", "COOKED")]       // 0 is falsy
    [InlineData("orderUp(!5)", "RAW")]          // non-zero is truthy
    [InlineData("orderUp(!\"\")", "COOKED")]   // empty string is falsy
    [InlineData("orderUp(!!5)", "COOKED")]      // double negation
    public void NegatesTruthiness(string source, string expected)
    {
        Assert.Equal(expected, Fc.Run(source));
    }

    [Fact]
    public void NegatesAComparison()
    {
        Assert.Equal("yes", Fc.Run("if (!(5 < 3)) { orderUp(\"yes\") } else { orderUp(\"no\") }"));
    }

    [Fact]
    public void BindsTighterThanLogicalAnd()
    {
        // !COOKED && COOKED  =>  (RAW) && COOKED  =>  RAW
        Assert.Equal("RAW", Fc.Run("orderUp(!COOKED && COOKED)"));
    }

    [Fact]
    public void InequalityStillLexesAsOneOperator()
    {
        Assert.Equal(new[] { "COOKED", "RAW" }, Fc.Lines(@"
            orderUp(1 != 2)
            orderUp(1 != 1)"));
    }

    [Fact]
    public void BareNotExpressionIsEchoedByTheRepl()
    {
        // Regression: RunRepl must treat a top-level `!x` as an expression to echo, not a statement.
        var interp = new Interpreter();
        var program = new Parser(new Lexer("!COOKED").Tokenise()).Parse();
        Assert.Equal(false, interp.RunRepl(program));
    }
}
