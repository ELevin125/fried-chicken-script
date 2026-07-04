namespace FriedChickenScript;

public static class Syntax
{
    // Keywords
    public const string Variable = "ingredient";
    public const string Function = "recipe";
    public const string Parameter = "withExtra:";
    public const string Object = "bucket";
    public const string Return = "serve";
    public const string If = "if";
    public const string Else = "else";
    public const string While = "fryWhile";
    public const string Break = "closeShop";

    // Builtins - runtime-provided functions, called like any recipe (name(args)). These are
    // NOT keywords — they are reserved identifiers so a program can't shadow them.
    public const string Print = "orderUp";
    public const string ReadIO = "takeOrder";
    public const string Random = "random";
    public const string RandomSeed = "randomSeed";

    // The receiver bound inside a bucket's recipe, referring to the current instance
    // (so a method can read/write its own fields, e.g. `myBucket.pieces`).
    public const string Self = "myBucket";

    // Constants
    public const string True = "COOKED";
    public const string False = "RAW";
    public const string Null = "EMPTY";


    // Operators
    public const string Addition = "+";
    public const string Subtraction = "-";
    public const string Multiplication = "*";
    public const string Division = "/";
    public const string Modulo = "%";

    // Logic Operators
    public const string And = "&&";
    public const string Or = "||";
    public const string Not = "!";
    public const string Equality = "==";
    public const string Inequality = "!=";
    public const string LessThan = "<";
    public const string GreaterThan = ">";
    public const string EqLessThan = "<=";
    public const string EqGreaterThan = ">=";

    // Other Elements
    public const string Assignment = "=";
    public const string Dot = ".";
}
