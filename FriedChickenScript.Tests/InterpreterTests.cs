using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class InterpreterTests
{
    // --- arithmetic / precedence ---

    [Fact]
    public void EvaluatesPrecedence()
    {
        Assert.Equal("14", Fc.Run("print(2 + 3 * 4)"));
    }

    [Fact]
    public void SubtractionIsLeftAssociative()
    {
        Assert.Equal("5", Fc.Run("print(10 - 2 - 3)"));
    }

    [Fact]
    public void VariablesAndReassignment()
    {
        Assert.Equal(new[] { "8", "20" }, Fc.Lines(@"
            ingredient x = 5
            ingredient y = x + 3
            print(y)
            y = y * 2 + 4
            print(y)"));
    }

    [Fact]
    public void DivideByZeroThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("print(1 / 0)"));
    }

    // --- strings ---

    [Fact]
    public void StringConcatDropsQuotes()
    {
        Assert.Equal("ab", Fc.Run("print(\"a\" + \"b\")"));
    }

    [Fact]
    public void StringNumberConcat()
    {
        Assert.Equal("n=5", Fc.Run("print(\"n=\" + 5)"));
    }

    // --- booleans / null ---

    [Fact]
    public void BooleansAndNullPrintInLanguage()
    {
        Assert.Equal(new[] { "COOKED", "RAW", "EMPTY" }, Fc.Lines(@"
            print(COOKED)
            print(RAW)
            print(EMPTY)"));
    }

    [Fact]
    public void ComparisonsProduceBooleans()
    {
        Assert.Equal(new[] { "COOKED", "RAW" }, Fc.Lines(@"
            print(5 > 3)
            print(5 < 3)"));
    }

    [Fact]
    public void LogicalOperators()
    {
        Assert.Equal(new[] { "COOKED", "RAW" }, Fc.Lines(@"
            print((1 < 2) || (2 < 1))
            print((1 < 2) && (2 < 1))"));
    }

    // --- control flow ---

    [Fact]
    public void IfElseTakesCorrectBranch()
    {
        Assert.Equal("yes", Fc.Run(@"
            if (5 > 3) { print(""yes"") } else { print(""no"") }"));
        Assert.Equal("no", Fc.Run(@"
            if (5 < 3) { print(""yes"") } else { print(""no"") }"));
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
            print(sum)"));
    }

    [Fact]
    public void WhileLoopHandlesManyIterationsWithoutStackOverflow()
    {
        Assert.Equal("500000", Fc.Run(@"
            ingredient i = 0
            fryWhile (i < 500000) { i = i + 1 }
            print(i)"));
    }

    // --- functions ---

    [Fact]
    public void FunctionWithParametersAndReturn()
    {
        Assert.Equal("7", Fc.Run(@"
            recipe addStuff withExtra: a, b { serve a + b }
            print(addStuff(3, 4))"));
    }

    [Fact]
    public void RecursionAndReturnFromInsideIf()
    {
        Assert.Equal("120", Fc.Run(@"
            recipe factorial withExtra: n {
                if (n < 2) { serve 1 }
                serve n * factorial(n - 1)
            }
            print(factorial(5))"));
    }

    [Fact]
    public void FunctionsCanBeCalledBeforeTheyAreDeclared()
    {
        Assert.Equal("9", Fc.Run(@"
            print(square(3))
            recipe square withExtra: n { serve n * n }"));
    }

    [Fact]
    public void WrongArgumentCountThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"
            recipe f withExtra: a, b { serve a }
            print(f(1))"));
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
            print(myOrder.recipeName)
            print(myOrder.pieces)
            print(myOrder.isSpicy)"));
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
            print(dinner.content)
            print(dinner.isSpicy)"));
    }

    [Fact]
    public void ObjectFieldsDoNotLeakIntoGlobalScope()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run(@"
            bucket B { ingredient secret = 99 }
            B b
            print(b.secret)
            print(secret)"));
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
            print(inner)"));
    }

    [Fact]
    public void AssigningToUndefinedVariableThrows()
    {
        Assert.Throws<FcRuntimeException>(() => Fc.Run("missing = 5"));
    }
}
