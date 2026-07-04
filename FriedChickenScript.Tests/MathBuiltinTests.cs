using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

// toNumber / min / max / abs / round / letItCook — the math and pacing builtins.
public class MathBuiltinTests
{
    // --- toNumber -------------------------------------------------------------

    [Fact]
    public void ToNumberParsesAWholeStringAsAnInt()
    {
        // int + int stays int, so "42" must come back as a genuine int.
        Assert.Equal("50", Fc.Run(@"orderUp(toNumber(""42"") + 8)").Trim());
    }

    [Fact]
    public void ToNumberParsesAFractionalString()
    {
        Assert.Equal("3.5", Fc.Run(@"orderUp(toNumber(""3.5""))").Trim());
    }

    [Fact]
    public void ToNumberResultCanIndexAList()
    {
        // Proves the whole-string case yields an int (list indexing needs a real int).
        Assert.Equal("b", Fc.Run(@"
            ingredient xs = [""a"", ""b"", ""c""]
            orderUp(xs[toNumber(""1"")])").Trim());
    }

    [Fact]
    public void ToNumberReturnsEmptyForNonNumbers()
    {
        Assert.Equal("EMPTY", Fc.Run(@"orderUp(toNumber(""chicken""))").Trim());
    }

    [Fact]
    public void ToNumberPassesNumbersStraightThrough()
    {
        Assert.Equal("7", Fc.Run(@"orderUp(toNumber(7))").Trim());
    }

    [Fact]
    public void ToNumberWithWrongArityIsAnError()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("toNumber()"));
    }

    // --- min / max ------------------------------------------------------------

    [Fact]
    public void MinReturnsTheSmallest()
    {
        Assert.Equal("1", Fc.Run("orderUp(min(5, 2, 9, 1))").Trim());
    }

    [Fact]
    public void MaxReturnsTheLargest()
    {
        Assert.Equal("9", Fc.Run("orderUp(max(5, 2, 9, 1))").Trim());
    }

    [Fact]
    public void MinAndMaxClampAValueToARange()
    {
        // The classic clamp: keep reputation within [0, 100].
        Assert.Equal("100", Fc.Run("orderUp(min(max(120, 0), 100))").Trim());
    }

    [Fact]
    public void MinNeedsAtLeastTwoArguments()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("min(5)"));
    }

    [Fact]
    public void MaxRejectsNonNumbers()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"max(1, ""two"")"));
    }

    // --- abs ------------------------------------------------------------------

    [Fact]
    public void AbsOfNegativeInt()
    {
        Assert.Equal("5", Fc.Run("orderUp(abs(-5))").Trim());
    }

    [Fact]
    public void AbsPreservesDouble()
    {
        Assert.Equal("3.5", Fc.Run("orderUp(abs(-3.5))").Trim());
    }

    // --- round ----------------------------------------------------------------

    [Fact]
    public void RoundToNearestWholeNumber()
    {
        Assert.Equal("5", Fc.Run("orderUp(round(5.4))").Trim());
    }

    [Fact]
    public void RoundTiesAwayFromZero()
    {
        Assert.Equal(new[] { "3", "-3" }, Fc.Lines(@"
            orderUp(round(2.5))
            orderUp(round(-2.5))"));
    }

    [Fact]
    public void RoundToDecimalPlaces()
    {
        Assert.Equal("1.23", Fc.Run("orderUp(round(1.2345, 2))").Trim());
    }

    [Fact]
    public void RoundWithTooManyArgumentsIsAnError()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("round(1, 2, 3)"));
    }

    // --- letItCook (sleep) ----------------------------------------------------

    [Fact]
    public void LetItCookRunsAndReturnsEmpty()
    {
        // A zero-length pause is a no-op; it returns EMPTY like the other side-effect builtins.
        Assert.Equal("EMPTY", Fc.Run("orderUp(letItCook(0))").Trim());
    }

    [Fact]
    public void LetItCookRejectsNegativeDurations()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("letItCook(-100)"));
    }

    [Fact]
    public void LetItCookRejectsNonNumbers()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"letItCook(""soon"")"));
    }

    // --- reservation ----------------------------------------------------------

    [Fact]
    public void TheNewBuiltinsAreReservedNames()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("ingredient min = 5"));
    }
}
