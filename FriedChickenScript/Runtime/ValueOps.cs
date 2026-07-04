using System.Globalization;

namespace FriedChickenScript;

// The single place that knows how FriedChickenScript values combine. Runtime values are
// plain C# objects: int, double, string, bool, null, or FcObject.
// int stays int, but any operation involving a double promotes number to a double.
public static class ValueOps
{
    // Apply a binary operator to two already-evaluated operands.
    public static object? Apply(string op, object? left, object? right)
    {
        switch (op)
        {
            case Syntax.Addition:       return Add(left, right);
            case Syntax.Subtraction:    return Arithmetic(op, left, right);
            case Syntax.Multiplication: return Arithmetic(op, left, right);
            case Syntax.Division:       return Arithmetic(op, left, right);

            case Syntax.And:            return Truthy(left) && Truthy(right);
            case Syntax.Or:             return Truthy(left) || Truthy(right);
            case Syntax.Equality:       return AreEqual(left, right);
            case Syntax.Inequality:     return !AreEqual(left, right);
            case Syntax.LessThan:       return AsDouble(left, op) <  AsDouble(right, op);
            case Syntax.GreaterThan:    return AsDouble(left, op) >  AsDouble(right, op);
            case Syntax.EqLessThan:     return AsDouble(left, op) <= AsDouble(right, op);
            case Syntax.EqGreaterThan:  return AsDouble(left, op) >= AsDouble(right, op);

            default:
                throw new FcRuntimeException($"Unknown operator '{op}'");
        }
    }

    // `+` is numeric addition when both sides are numbers, otherwise string concatenation.
    private static object Add(object? left, object? right)
    {
        if (left is int li && right is int ri)
        {
            return li + ri;
        }
        if (IsNumber(left) && IsNumber(right))
        {
            return AsDouble(left, Syntax.Addition) + AsDouble(right, Syntax.Addition);
        }
        if (left is string || right is string)
        {
            return Stringify(left) + Stringify(right);
        }
        // Neither numeric nor a string: surface a clear type error.
        return AsDouble(left, Syntax.Addition) + AsDouble(right, Syntax.Addition);
    }

    // `-`, `*`, `/`: integer math when both operands are ints, otherwise promote to double.
    private static object Arithmetic(string op, object? left, object? right)
    {
        if (left is int li && right is int ri)
        {
            switch (op)
            {
                case Syntax.Subtraction:    return li - ri;
                case Syntax.Multiplication: return li * ri;
                case Syntax.Division:
                    if (ri == 0)
                    {
                        throw new FcRuntimeException("Division by zero");
                    }
                    return li / ri;
            }
        }

        double l = AsDouble(left, op);
        double r = AsDouble(right, op);
        switch (op)
        {
            case Syntax.Subtraction:    return l - r;
            case Syntax.Multiplication: return l * r;
            case Syntax.Division:
                if (r == 0)
                {
                    throw new FcRuntimeException("Division by zero");
                }
                return l / r;
        }

        throw new FcRuntimeException($"Unknown operator '{op}'");
    }

    private static bool IsNumber(object? value) => value is int || value is double;

    private static double AsDouble(object? value, string op)
    {
        if (value is int i)
        {
            return i;
        }
        if (value is double d)
        {
            return d;
        }
        if (value is string s)
        {
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                return parsed;
            }
            throw new FcRuntimeException($"Operator '{op}' expects a number but got {Describe(value)}");
        }

        // Treat null as 0 in arithmetic operations, similar to JS
        if (value is null)
        {
            return 0;
        }
        throw new FcRuntimeException($"Operator '{op}' expects a number but got {Describe(value)}");
    }

    // Unary minus: negate a number, preserving int vs double.
    public static object Negate(object? value)
    {
        if (value is int i)
        {
            return -i;
        }
        if (value is double d)
        {
            return -d;
        }
        if (value is string s)
        {
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                return -parsed;
            }
            throw new FcRuntimeException($"Operator '-' expects a number but got {Describe(value)}");
        }

        // Treat null as 0 in arithmetic operations, similar to JS
        if (value is null)
        {
            return 0;
        }
        throw new FcRuntimeException($"Operator '-' expects a number but got {Describe(value)}");
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }
        if (IsNumber(left) && IsNumber(right))
        {
            return AsDouble(left, Syntax.Equality) == AsDouble(right, Syntax.Equality);
        }
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
            double d => d != 0,
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
            double d => d.ToString(CultureInfo.InvariantCulture),
            List<object?> list => "[" + string.Join(", ", list.Select(Stringify)) + "]",
            _ => value.ToString() ?? "",
        };
    }

    private static string Describe(object? value)
    {
        if (value is null)
        {
            return Syntax.Null;
        }
        return $"{value.GetType().Name.ToLowerInvariant()} '{Stringify(value)}'";
    }
}
