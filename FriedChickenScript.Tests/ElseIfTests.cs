using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class ElseIfTests
{
    // Grade a score by falling through an else-if chain.
    private static string Grade(int score) =>
        Fc.Run(@"
            ingredient score = " + score + @"
            if (score >= 90) {
                orderUp(""A"")
            }
            else if (score >= 80) {
                orderUp(""B"")
            }
            else if (score >= 70) {
                orderUp(""C"")
            }
            else {
                orderUp(""F"")
            }");

    [Theory]
    [InlineData(95, "A")]
    [InlineData(85, "B")]
    [InlineData(75, "C")]
    [InlineData(50, "F")]
    public void ChainPicksTheFirstMatchingBranch(int score, string expected)
    {
        Assert.Equal(expected, Grade(score));
    }

    [Fact]
    public void OnlyOneBranchRuns()
    {
        // Both conditions are true, but the chain must stop at the first.
        Assert.Equal("first", Fc.Run(@"
            if (1 < 2) {
                orderUp(""first"")
            }
            else if (2 < 3) {
                orderUp(""second"")
            }"));
    }

    [Fact]
    public void ElseIfWithoutFinalElseCanMatchNothing()
    {
        // No branch matches and there's no trailing else, so nothing prints.
        Assert.Equal("", Fc.Run(@"
            if (RAW) {
                orderUp(""a"")
            }
            else if (RAW) {
                orderUp(""b"")
            }"));
    }

    [Fact]
    public void ElseIfNestsInsideLoopsAndBlocks()
    {
        Assert.Equal(new[] { "low", "mid", "high" }, Fc.Lines(@"
            ingredient i = 0
            fryWhile (i < 3) {
                if (i == 0) {
                    orderUp(""low"")
                }
                else if (i == 1) {
                    orderUp(""mid"")
                }
                else {
                    orderUp(""high"")
                }
                i = i + 1
            }"));
    }
}
