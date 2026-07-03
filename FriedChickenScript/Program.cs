using FriedChickenScript;

class Program
{
    static int Main(string[] args)
    {
        bool debug = args.Contains("--debug");
        string? path = args.FirstOrDefault(a => !a.StartsWith("--"));

        if (path == null)
        {
            Console.Error.WriteLine("Usage: FriedChickenScript [--debug] <script.fc>");
            return 1;
        }

        string script = File.ReadAllText(path);

        try
        {
            List<Token> tokens = new Lexer(script).Tokenise();
            if (debug)
            {
                Console.WriteLine("--- tokens ---");
                tokens.ForEach(t => Console.WriteLine(t));
            }

            ASTNode ast = new Parser(tokens).Parse();
            if (debug)
            {
                Console.WriteLine("--- syntax tree ---");
                PrintTree(ast);
                Console.WriteLine("--- output ---");
            }

            new Interpreter().Run(ast);
            return 0;
        }
        catch (FcParseException e)
        {
            Console.Error.WriteLine($"Syntax error: {e.Message}");
            return 1;
        }
        catch (FcRuntimeException e)
        {
            Console.Error.WriteLine($"Runtime error: {e.Message}");
            return 1;
        }
    }

    static void PrintTree(ASTNode node, string indent = "", bool last = true)
    {
        Console.WriteLine(indent + "+- " + node);
        indent += last ? "   " : "|  ";
        for (int i = 0; i < node.Children.Count; i++)
        {
            PrintTree(node.Children[i], indent, i == node.Children.Count - 1);
        }
    }
}
