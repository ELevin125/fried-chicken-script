using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class ArrayTests
{
    // --- literals & printing ---

    [Theory]
    [InlineData("orderUp([1, 2, 3])", "[1, 2, 3]")]
    [InlineData("orderUp([])", "[]")]
    [InlineData("orderUp([\"wing\", \"thigh\"])", "[wing, thigh]")]
    [InlineData("orderUp([1, \"two\", COOKED, 3.5])", "[1, two, COOKED, 3.5]")]
    public void ListLiterals(string source, string expected)
    {
        Assert.Equal(expected, Fc.Run(source));
    }

    // --- get / set ---

    [Fact]
    public void IndexGet()
    {
        Assert.Equal("20", Fc.Run(@"
            ingredient a = [10, 20, 30]
            orderUp(a[1])"));
    }

    [Fact]
    public void IndexSet()
    {
        Assert.Equal(new[] { "99", "3" }, Fc.Lines(@"
            ingredient a = [10, 20, 30]
            a[0] = 99
            orderUp(a[0])
            orderUp(a.length)"));
    }

    [Fact]
    public void NestedIndexing()
    {
        Assert.Equal("3", Fc.Run(@"
            ingredient m = [[1, 2], [3, 4]]
            orderUp(m[1][0])"));
    }

    // --- length ---

    [Theory]
    [InlineData("orderUp([1, 2, 3].length)", "3")]
    [InlineData("orderUp([].length)", "0")]
    public void Length(string source, string expected)
    {
        Assert.Equal(expected, Fc.Run(source));
    }

    // --- add / remove ---

    [Fact]
    public void AddAppends()
    {
        Assert.Equal(new[] { "4", "40" }, Fc.Lines(@"
            ingredient a = [10, 20, 30]
            a.add(40)
            orderUp(a.length)
            orderUp(a[3])"));
    }

    [Fact]
    public void RemoveReturnsRemovedValueAndShrinks()
    {
        Assert.Equal(new[] { "10", "2", "20" }, Fc.Lines(@"
            ingredient a = [10, 20, 30]
            ingredient gone = a.remove(0)
            orderUp(gone)
            orderUp(a.length)
            orderUp(a[0])"));
    }

    // --- semantics ---

    [Fact]
    public void ListsAreSharedByReference()
    {
        Assert.Equal("3", Fc.Run(@"
            ingredient a = [1, 2]
            ingredient b = a
            b.add(3)
            orderUp(a.length)"));
    }

    [Fact]
    public void CanIterateWithAWhileLoop()
    {
        Assert.Equal("60", Fc.Run(@"
            ingredient a = [10, 20, 30]
            ingredient i = 0
            ingredient sum = 0
            fryWhile (i < a.length) {
                sum = sum + a[i]
                i = i + 1
            }
            orderUp(sum)"));
    }

    // --- errors ---

    [Theory]
    [InlineData("ingredient a = [1, 2]\norderUp(a[5])")]        // read out of range
    [InlineData("ingredient a = [1, 2]\na[5] = 9")]             // write out of range
    [InlineData("ingredient a = [1, 2]\norderUp(a[1.5])")]      // non-integer index
    [InlineData("ingredient n = 5\norderUp(n[0])")]             // indexing a non-list
    [InlineData("ingredient n = 5\nn.add(1)")]                  // method on a non-list
    [InlineData("ingredient a = [1]\na.slurp()")]               // unknown method
    public void BadListOperationsThrow(string source)
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(source));
    }

    // --- REPL ---

    [Fact]
    public void BareListLiteralIsEchoedByTheRepl()
    {
        var interp = new Interpreter();
        var program = new Parser(new Lexer("[1, 2, 3]").Tokenise()).Parse();
        var result = interp.RunRepl(program);
        var list = Assert.IsType<List<object?>>(result);
        Assert.Equal(3, list.Count);
    }
}
