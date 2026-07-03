namespace FriedChickenScript;

// Thrown by `serve` (return) to unwind out of any nested blocks/loops straight back to
// the function-call boundary, carrying the returned value. Phase 2 will add sibling
// signals (BreakException / ContinueException) for loop control.
public class ReturnException : Exception
{
    public object? Value { get; }

    public ReturnException(object? value)
    {
        Value = value;
    }
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
