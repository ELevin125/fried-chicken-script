using FriedChickenScript;
using Xunit;

// Console.SetOut is process-global, so serialize tests rather than let them interleave.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace FriedChickenScript.Tests;

public static class Fc
{
    // Run a script and return everything it printed (newlines normalised, trailing blank
    // lines trimmed).
    public static string Run(string script)
    {
        var tokens = new Lexer(script).Tokenise();
        var ast = new Parser(tokens).Parse();

        var writer = new StringWriter();
        var previous = Console.Out;
        Console.SetOut(writer);
        try
        {
            new Interpreter().Run(ast);
        }
        finally
        {
            Console.SetOut(previous);
        }

        return writer.ToString().Replace("\r\n", "\n").TrimEnd('\n');
    }

    public static string[] Lines(string script) =>
        Run(script).Split('\n', StringSplitOptions.RemoveEmptyEntries);

    // Parse a script and return the initializer expression of its first declaration,
    // e.g. the `2 + 3 * 4` tree from `ingredient a = 2 + 3 * 4`.
    public static ASTNode FirstDeclInit(string script)
    {
        var tokens = new Lexer(script).Tokenise();
        var program = new Parser(tokens).Parse();
        return program.Children[0].Children[0];
    }
}
