namespace FriedChickenScript;

// The single place that knows how FriedChickenScript values combine. Runtime values are
// plain C# objects: int, string, bool, null, or FcObject (arrays/floats arrive in Phase 2).
// Centralising the type rules here kills the old "re-parse ints from strings" approach.
public static class ValueOps
{
    // Apply a binary operator to two already-evaluated operands.
    public static object? Apply(string op, object? left, object? right)
    {
        switch (op)
        {
            case Syntax.Addition:       return Add(left, right);
            case Syntax.Subtraction:    return AsInt(left, op) - AsInt(right, op);
            case Syntax.Multiplication: return AsInt(left, op) * AsInt(right, op);
            case Syntax.Division:
                int divisor = AsInt(right, op);
                if (divisor == 0)
                    throw new FcRuntimeException("Division by zero");
                return AsInt(left, op) / divisor;

            case Syntax.And:            return Truthy(left) && Truthy(right);
            case Syntax.Or:             return Truthy(left) || Truthy(right);
            case Syntax.Equality:       return AreEqual(left, right);
            case Syntax.Inequality:     return !AreEqual(left, right);
            case Syntax.LessThan:       return AsInt(left, op) <  AsInt(right, op);
            case Syntax.GreaterThan:    return AsInt(left, op) >  AsInt(right, op);
            case Syntax.EqLessThan:     return AsInt(left, op) <= AsInt(right, op);
            case Syntax.EqGreaterThan:  return AsInt(left, op) >= AsInt(right, op);

            default:
                throw new FcRuntimeException($"Unknown operator '{op}'");
        }
    }

    // `+` is numeric addition when both sides are numbers, otherwise string concatenation.
    private static object Add(object? left, object? right)
    {
        if (left is int l && right is int r)
            return l + r;
        if (left is string || right is string)
            return Stringify(left) + Stringify(right);
        return AsInt(left, Syntax.Addition) + AsInt(right, Syntax.Addition);
    }

    private static int AsInt(object? value, string op)
    {
        if (value is int i)
            return i;
        throw new FcRuntimeException($"Operator '{op}' expects a number but got {Describe(value)}");
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (left is null || right is null)
            return left is null && right is null;
        return left.Equals(right);
    }

    // Which values count as "true" for conditions (if / fryWhile / && / ||).
    public static bool Truthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            string s => s.Length > 0,
            _ => true,
        };
    }

    // Human-facing rendering used by `print`, concatenation, and FcObject.ToString.
    // Booleans and null render in-language (COOKED / RAW / EMPTY).
    public static string Stringify(object? value)
    {
        return value switch
        {
            null => Syntax.Null,
            bool b => b ? Syntax.True : Syntax.False,
            _ => value.ToString() ?? "",
        };
    }

    private static string Describe(object? value)
    {
        if (value is null)
            return Syntax.Null;
        return $"{value.GetType().Name.ToLowerInvariant()} '{Stringify(value)}'";
    }
}
