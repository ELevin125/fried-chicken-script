using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class DecimalTests
{
    [Fact]
    public void LexesDecimalAsSingleNumberToken()
    {
        var tokens = new Lexer("3.14").Tokenise();
        var token = Assert.Single(tokens);
        Assert.Equal(TokenType.Number, token.Type);
        Assert.Equal("3.14", token.Value);
    }

    [Fact]
    public void PrintsDecimal()
    {
        Assert.Equal("3.14", Fc.Run("print(3.14)"));
    }

    [Fact]
    public void DecimalArithmeticPromotes()
    {
        Assert.Equal("3.5", Fc.Run("print(2 + 1.5)"));
    }

    [Fact]
    public void IntArithmeticStaysInt()
    {
        Assert.Equal("2", Fc.Run("print(10 / 4)")); // integer division
        Assert.Equal("14", Fc.Run("print(2 + 3 * 4)"));
    }

    [Theory]
    [InlineData("print(10.0 / 4)", "2.5")]
    [InlineData("print(10 / 4.0)", "2.5")]
    [InlineData("print(3.0 * 2)", "6")]     // whole-valued double prints without .0
    public void MixedDivisionAndMultiplyPromote(string source, string expected)
    {
        Assert.Equal(expected, Fc.Run(source));
    }

    [Fact]
    public void WholeValuedDoublePrintsCleanly()
    {
        Assert.Equal("4", Fc.Run("print(1.5 + 2.5)"));
    }

    [Fact]
    public void NumericEqualityAcrossIntAndDouble()
    {
        Assert.Equal(new[] { "COOKED", "RAW" }, Fc.Lines(@"
            print(2.0 == 2)
            print(2.5 == 2)"));
    }

    [Fact]
    public void DecimalComparison()
    {
        Assert.Equal(new[] { "COOKED", "RAW" }, Fc.Lines(@"
            print(1.5 < 2)
            print(2.5 < 2)"));
    }

    [Fact]
    public void ZeroDoubleIsFalsy()
    {
        Assert.Equal("n", Fc.Run("if (0.0) { print(\"y\") } else { print(\"n\") }"));
    }

    [Fact]
    public void DivideByZeroThrowsForDoubles()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("print(1.0 / 0)"));
    }

    [Fact]
    public void DecimalsWorkAsVariablesAndInLoops()
    {
        Assert.Equal("1.5", Fc.Run(@"
            ingredient total = 0.0
            ingredient i = 0
            fryWhile (i < 3) {
                total = total + 0.5
                i = i + 1
            }
            print(total)"));
    }
}
