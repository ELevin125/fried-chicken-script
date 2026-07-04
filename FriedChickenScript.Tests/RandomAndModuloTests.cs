using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class ModuloTests
{
    [Theory]
    [InlineData("orderUp(10 % 3)", "1")]
    [InlineData("orderUp(10 % 2)", "0")]
    [InlineData("orderUp(7 % 10)", "7")]
    public void IntegerModulo(string source, string expected)
    {
        Assert.Equal(expected, Fc.Run(source));
    }

    [Fact]
    public void ModuloPromotesToDouble()
    {
        Assert.Equal("1.5", Fc.Run("orderUp(5.5 % 2)"));
    }

    [Fact]
    public void ModuloBindsTighterThanAddition()
    {
        // 1 + (10 % 3) == 1 + 1 == 2
        Assert.Equal("2", Fc.Run("orderUp(1 + 10 % 3)"));
    }

    [Fact]
    public void ModuloBindsLikeMultiplication()
    {
        // Same precedence as * and /, left-associative: (20 / 3) % 4 == 6 % 4 == 2
        Assert.Equal("2", Fc.Run("orderUp(20 / 3 % 4)"));
    }

    [Fact]
    public void ModuloByZeroThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp(5 % 0)"));
    }
}

public class RandomTests
{
    [Fact]
    public void RandomWithBoundStaysInRange()
    {
        // Every draw of random(5) must be one of 0..4, so their max stays below 5
        // and the min stays at or above 0 across many samples.
        Assert.Equal("COOKED", Fc.Run(@"
            randomSeed(1)
            ingredient i = 0
            ingredient ok = COOKED
            fryWhile (i < 200) {
                ingredient r = random(5)
                if (r < 0 || r >= 5) {
                    ok = RAW
                }
                i = i + 1
            }
            orderUp(ok)"));
    }

    [Fact]
    public void NoArgRandomIsUnitInterval()
    {
        Assert.Equal("COOKED", Fc.Run(@"
            randomSeed(1)
            ingredient r = random()
            orderUp(r >= 0 && r < 1)"));
    }

    [Fact]
    public void SameSeedReproducesSameSequence()
    {
        Assert.Equal("COOKED", Fc.Run(@"
            randomSeed(42)
            ingredient a = random(1000000)
            randomSeed(42)
            ingredient b = random(1000000)
            orderUp(a == b)"));
    }

    [Fact]
    public void RandomBoundMustBePositive()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp(random(0))"));
    }

    [Fact]
    public void RandomRejectsTooManyArguments()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp(random(1, 2))"));
    }
}
