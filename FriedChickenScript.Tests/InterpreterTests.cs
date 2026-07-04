using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class InterpreterTests
{
    // --- arithmetic / precedence ---

    [Fact]
    public void EvaluatesPrecedence()
    {
        Assert.Equal("14", Fc.Run("orderUp(2 + 3 * 4)"));
    }

    [Fact]
    public void SubtractionIsLeftAssociative()
    {
        Assert.Equal("5", Fc.Run("orderUp(10 - 2 - 3)"));
    }

    [Fact]
    public void VariablesAndReassignment()
    {
        Assert.Equal(new[] { "8", "20" }, Fc.Lines(@"
            ingredient x = 5
            ingredient y = x + 3
            orderUp(y)
            y = y * 2 + 4
            orderUp(y)"));
    }

    [Fact]
    public void DivideByZeroThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("orderUp(1 / 0)"));
    }

    // --- strings ---

    [Fact]
    public void StringConcatDropsQuotes()
    {
        Assert.Equal("ab", Fc.Run("orderUp(\"a\" + \"b\")"));
    }

    [Fact]
    public void StringNumberConcat()
    {
        Assert.Equal("n=5", Fc.Run("orderUp(\"n=\" + 5)"));
    }

    // --- booleans / null ---

    [Fact]
    public void BooleansAndNullPrintInLanguage()
    {
        Assert.Equal(new[] { "COOKED", "RAW", "EMPTY" }, Fc.Lines(@"
            orderUp(COOKED)
            orderUp(RAW)
            orderUp(EMPTY)"));
    }

    [Fact]
    public void ComparisonsProduceBooleans()
    {
        Assert.Equal(new[] { "COOKED", "RAW" }, Fc.Lines(@"
            orderUp(5 > 3)
            orderUp(5 < 3)"));
    }

    [Fact]
    public void LogicalOperators()
    {
        Assert.Equal(new[] { "COOKED", "RAW" }, Fc.Lines(@"
            orderUp((1 < 2) || (2 < 1))
            orderUp((1 < 2) && (2 < 1))"));
    }

    // --- control flow ---

    [Fact]
    public void IfElseTakesCorrectBranch()
    {
        Assert.Equal("yes", Fc.Run(@"
            if (5 > 3) { orderUp(""yes"") } else { orderUp(""no"") }"));
        Assert.Equal("no", Fc.Run(@"
            if (5 < 3) { orderUp(""yes"") } else { orderUp(""no"") }"));
    }

    [Fact]
    public void WhileLoopSumsRange()
    {
        Assert.Equal("10", Fc.Run(@"
            ingredient i = 0
            ingredient sum = 0
            fryWhile (i < 5) {
                sum = sum + i
                i = i + 1
            }
            orderUp(sum)"));
    }

    [Fact]
    public void WhileLoopHandlesManyIterationsWithoutStackOverflow()
    {
        Assert.Equal("500000", Fc.Run(@"
            ingredient i = 0
            fryWhile (i < 500000) { i = i + 1 }
            orderUp(i)"));
    }

    // --- functions ---

    [Fact]
    public void FunctionWithParametersAndReturn()
    {
        Assert.Equal("7", Fc.Run(@"
            recipe addStuff withExtra: a, b { serve a + b }
            orderUp(addStuff(3, 4))"));
    }

    [Fact]
    public void RecursionAndReturnFromInsideIf()
    {
        Assert.Equal("120", Fc.Run(@"
            recipe factorial withExtra: n {
                if (n < 2) { serve 1 }
                serve n * factorial(n - 1)
            }
            orderUp(factorial(5))"));
    }

    [Fact]
    public void FunctionsCanBeCalledBeforeTheyAreDeclared()
    {
        Assert.Equal("9", Fc.Run(@"
            orderUp(square(3))
            recipe square withExtra: n { serve n * n }"));
    }

    [Fact]
    public void WrongArgumentCountThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"
            recipe f withExtra: a, b { serve a }
            orderUp(f(1))"));
    }

    // --- objects (buckets) ---

    [Fact]
    public void ObjectDefaultsAndFieldReadWrite()
    {
        Assert.Equal(new[] { "Original", "12", "COOKED" }, Fc.Lines(@"
            bucket Order {
                ingredient recipeName = ""Original""
                ingredient pieces = 8
                ingredient isSpicy = RAW
            }
            Order myOrder
            myOrder.pieces = 12
            myOrder.isSpicy = COOKED
            orderUp(myOrder.recipeName)
            orderUp(myOrder.pieces)
            orderUp(myOrder.isSpicy)"));
    }

    [Fact]
    public void ObjectReturnedFromFunction()
    {
        Assert.Equal(new[] { "chicken x 12", "COOKED" }, Fc.Lines(@"
            bucket Meal {
                ingredient content = """"
                ingredient isSpicy = RAW
            }
            recipe cook withExtra: pieces, spicy {
                Meal m
                m.content = ""chicken x "" + pieces
                m.isSpicy = spicy
                serve m
            }
            Meal dinner = cook(12, COOKED)
            orderUp(dinner.content)
            orderUp(dinner.isSpicy)"));
    }

    [Fact]
    public void ObjectFieldsDoNotLeakIntoGlobalScope()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"
            bucket B { ingredient secret = 99 }
            B b
            orderUp(b.secret)
            orderUp(secret)"));
    }

    [Fact]
    public void TypeMismatchOnInitializationThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"
            bucket A { ingredient x = 1 }
            bucket B { ingredient y = 2 }
            recipe makeA { A a serve a }
            B b = makeA()"));
    }

    // --- scoping ---

    [Fact]
    public void BlockLocalsDoNotEscapeTheirScope()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"
            if (COOKED) { ingredient inner = 1 }
            orderUp(inner)"));
    }

    [Fact]
    public void AssigningToUndefinedVariableThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("missing = 5"));
    }
}
