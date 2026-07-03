using System.Text;

namespace FriedChickenScript;

// Interactive shell (REPL). Keeps one Interpreter alive so definitions persist across
// lines. A statement runs for its effect; a bare expression has its value echoed.
public class Repl
{
    private const string Version = "v0.1";

    private Interpreter interpreter = new();

    public void Run()
    {
        Console.WriteLine($"FriedChicken Script {Version} (interactive shell)");
        Console.WriteLine("Type .help for commands, .exit to quit.");

        while (true)
        {
            string? input = ReadStatement();
            if (input == null) // Ctrl+D / end of input
            {
                break;
            }

            input = input.Trim();
            if (input.Length == 0)
            {
                continue;
            }

            if (input.StartsWith("."))
            {
                bool keepGoing = RunMetaCommand(input);
                if (!keepGoing)
                {
                    break;
                }
                continue;
            }

            Evaluate(input);
        }
    }

    // Read a full statement, continuing across lines with a "...." prompt until the
    // braces/parens balance (so multi-line recipes, buckets, and blocks work).
    private static string? ReadStatement()
    {
        Console.Write("fcs> ");
        string? line = Console.ReadLine();
        if (line == null)
        {
            return null;
        }

        string trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith("."))
        {
            return line;
        }

        StringBuilder buffer = new(line);
        while (!IsBalanced(buffer.ToString()))
        {
            Console.Write(".... ");
            string? more = Console.ReadLine();
            if (more == null || more.Trim().Length == 0)
            {
                // EOF or a blank line submits what we have (a parse error will surface
                // if it's genuinely incomplete) so the prompt can't get stuck.
                break;
            }
            buffer.Append('\n').Append(more);
        }
        return buffer.ToString();
    }

    // Balanced when every '(' and '{' has a matching close. Counts lexer tokens, not raw
    // characters, so brackets inside strings and comments don't throw the count off.
    public static bool IsBalanced(string source)
    {
        int depth = 0;
        try
        {
            foreach (Token token in new Lexer(source).Tokenise())
            {
                if (token.Type == TokenType.LeftBrace || token.Type == TokenType.LeftParen)
                {
                    depth++;
                }
                else if (token.Type == TokenType.RightBrace || token.Type == TokenType.RightParen)
                {
                    depth--;
                }
            }
        }
        catch (FcParseException)
        {
            // e.g. an unterminated string: treat as complete so the error surfaces on parse.
            return true;
        }
        return depth <= 0;
    }

    private void Evaluate(string source)
    {
        try
        {
            List<Token> tokens = new Lexer(source).Tokenise();
            ASTNode program = new Parser(tokens).Parse();
            object? result = interpreter.RunRepl(program);
            if (result != null)
            {
                Console.WriteLine($"=> {ValueOps.Stringify(result)}");
            }
        }
        catch (FcParseException e)
        {
            Console.WriteLine($"Syntax error: {e.Message}");
        }
        catch (FcRuntimeException e)
        {
            Console.WriteLine($"Runtime error: {e.Message}");
        }
    }

    // Returns false when the REPL should exit.
    private bool RunMetaCommand(string input)
    {
        switch (input)
        {
            case ".exit":
            case ".quit":
                return false;

            case ".help":
                PrintHelp();
                return true;

            case ".clear":
                interpreter = new Interpreter();
                Console.WriteLine("Session cleared.");
                return true;

            case ".vars":
                PrintVars();
                return true;

            default:
                Console.WriteLine($"Unknown command '{input}'. Type .help for the list.");
                return true;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("  .help    show this help");
        Console.WriteLine("  .vars    list defined variables, recipes, and buckets");
        Console.WriteLine("  .clear   reset the session");
        Console.WriteLine("  .exit    quit (Ctrl+D also works)");
        Console.WriteLine("Type any statement to run it; a bare expression prints its value.");
    }

    private void PrintVars()
    {
        PrintNames("variables", interpreter.DefinedVariables);
        PrintNames("recipes", interpreter.DefinedRecipes);
        PrintNames("buckets", interpreter.DefinedBuckets);
    }

    private static void PrintNames(string label, IReadOnlyCollection<string> names)
    {
        string body = names.Count == 0 ? "(none)" : string.Join(", ", names);
        Console.WriteLine($"  {label}: {body}");
    }
}
