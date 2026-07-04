using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

// `closeShop` is FriedChicken's break: stop the nearest enclosing fryWhile loop.
public class BreakTests
{
    [Fact]
    public void BreakStopsTheLoopEarly()
    {
        Assert.Equal(new[] { "0", "1", "2" }, Fc.Lines(@"
            ingredient i = 0
            fryWhile (i < 10) {
                if (i == 3) {
                    closeShop
                }
                orderUp(i)
                i = i + 1
            }"));
    }

    [Fact]
    public void BreakOnlyExitsTheInnermostLoop()
    {
        // The inner loop breaks after inner == 0, but the outer loop keeps going.
        Assert.Equal(new[] { "0-0", "1-0" }, Fc.Lines(@"
            ingredient outer = 0
            fryWhile (outer < 2) {
                ingredient inner = 0
                fryWhile (inner < 5) {
                    if (inner == 1) {
                        closeShop
                    }
                    orderUp(outer + ""-"" + inner)
                    inner = inner + 1
                }
                outer = outer + 1
            }"));
    }

    [Fact]
    public void BreakExitsTheLoopButNotTheRecipe()
    {
        // closeShop leaves the loop; the recipe carries on and prints "done".
        Assert.Equal(new[] { "0", "1", "done" }, Fc.Lines(@"
            recipe countUpTo withExtra: limit {
                ingredient i = 0
                fryWhile (i < 100) {
                    if (i >= limit) {
                        closeShop
                    }
                    orderUp(i)
                    i = i + 1
                }
                orderUp(""done"")
            }
            countUpTo(2)"));
    }

    [Fact]
    public void BreakOutsideAnyLoopIsAnError()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("closeShop"));
    }

    [Fact]
    public void BreakDoesNotCrossARecipeCallBoundary()
    {
        // closeShop inside a recipe that isn't itself looping can't break the caller's loop.
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"
            recipe bail {
                closeShop
            }
            ingredient i = 0
            fryWhile (i < 3) {
                bail()
                i = i + 1
            }"));
    }
}
