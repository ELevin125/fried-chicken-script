namespace FriedChickenScript;

// Thrown by `serve` (return) to unwind out of any nested blocks/loops straight back to
// the function-call boundary, carrying the returned value.
public class ReturnException : Exception
{
    public object? Value { get; }

    public ReturnException(object? value)
    {
        Value = value;
    }
}

// Thrown by `closeShop` (break) to unwind out to the nearest enclosing fryWhile loop,
// which stops iterating. Never crosses a recipe-call boundary or reaches top level —
// those turn it into a clear "outside a loop" error.
public class BreakException : Exception
{
}

// Any error raised while executing a program: bad types, undefined names, division by
// zero, etc. Distinct from parse/lex errors so callers can tell them apart later.
public class FcRuntimeException : Exception
{
    public FcRuntimeException(string message) : base(message) { }
}

// Raised while lexing or parsing: unexpected character or unexpected token.
public class FcParseException : Exception
{
    public FcParseException(string message) : base(message) { }
}
