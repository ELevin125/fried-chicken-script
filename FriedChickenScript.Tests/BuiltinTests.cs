using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

// orderUp (print) and takeOrder (input) are now runtime builtins rather than keywords —
// ordinary calls dispatched by name, alongside random / randomSeed.
public class BuiltinTests
{
    [Fact]
    public void TakeOrderReadsALineOfInput()
    {
        var previousIn = Console.In;
        Console.SetIn(new StringReader("Hot Wings\n"));
        try
        {
            Assert.Equal("Hot Wings", Fc.Run("orderUp(takeOrder())"));
        }
        finally
        {
            Console.SetIn(previousIn);
        }
    }

    [Fact]
    public void TakeOrderCoercesNumericInputInArithmetic()
    {
        var previousIn = Console.In;
        Console.SetIn(new StringReader("10\n"));
        try
        {
            Assert.Equal("8", Fc.Run("orderUp(takeOrder() - 2)"));
        }
        finally
        {
            Console.SetIn(previousIn);
        }
    }

    // orderUp is now an expression that prints and yields EMPTY, so nesting it prints the
    // inner value and then EMPTY.
    [Fact]
    public void PrintReturnsEmpty()
    {
        Assert.Equal(new[] { "hi", "EMPTY" }, Fc.Lines("orderUp(orderUp(\"hi\"))"));
    }

    [Fact]
    public void PrintRequiresExactlyOneArgument()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp()"));
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp(1, 2)"));
    }

    [Fact]
    public void TakeOrderRejectsArguments()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp(takeOrder(5))"));
    }

    [Fact]
    public void CannotRedefineBuiltinAsRecipe()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("recipe orderUp { serve 1 }"));
    }

    [Fact]
    public void CannotRedefineBuiltinAsVariable()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("ingredient random = 5"));
    }

    [Fact]
    public void CallingUndefinedFunctionStillErrors()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp(notARecipe())"));
    }
}
