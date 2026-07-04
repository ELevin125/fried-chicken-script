using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class ReplTests
{
    private static ASTNode Parse(string source) =>
        new Parser(new Lexer(source).Tokenise()).Parse();

    // --- RunRepl: persistent state + expression echo value ---

    [Fact]
    public void BareExpressionReturnsItsValue()
    {
        var interp = new Interpreter();
        Assert.Equal(5, interp.RunRepl(Parse("2 + 3")));
    }

    [Fact]
    public void StatementsReturnNull()
    {
        var interp = new Interpreter();
        Assert.Null(interp.RunRepl(Parse("ingredient x = 5")));
    }

    [Fact]
    public void StatePersistsAcrossFragments()
    {
        var interp = new Interpreter();
        interp.RunRepl(Parse("ingredient x = 5"));
        Assert.Equal(6, interp.RunRepl(Parse("x + 1")));
    }

    [Fact]
    public void RecipesPersistAndAreCallable()
    {
        var interp = new Interpreter();
        interp.RunRepl(Parse("recipe sq withExtra: n { serve n * n }"));
        Assert.Equal(16, interp.RunRepl(Parse("sq(4)")));
    }

    [Fact]
    public void VoidLikeCallReturnsNull()
    {
        var interp = new Interpreter();
        interp.RunRepl(Parse("recipe noop { serve EMPTY }"));
        Assert.Null(interp.RunRepl(Parse("noop()")));
    }

    [Fact]
    public void DefinedNamesAreReported()
    {
        var interp = new Interpreter();
        interp.RunRepl(Parse("ingredient x = 1"));
        interp.RunRepl(Parse("recipe f { serve 1 }"));
        interp.RunRepl(Parse("bucket B { ingredient y = 2 }"));

        Assert.Contains("x", interp.DefinedVariables);
        Assert.Contains("f", interp.DefinedRecipes);
        Assert.Contains("B", interp.DefinedBuckets);
    }

    // --- IsBalanced: multi-line continuation detection ---

    [Theory]
    [InlineData("ingredient x = 5", true)]
    [InlineData("orderUp(x)", true)]
    [InlineData("if (x) { orderUp(1) }", true)]
    [InlineData("if (x) {", false)]
    [InlineData("recipe f {", false)]
    [InlineData("ingredient x = (2 + 3", false)]
    [InlineData("\"a string with } and )\"", true)]
    public void BraceBalanceDetection(string source, bool balanced)
    {
        Assert.Equal(balanced, Repl.IsBalanced(source));
    }
}
