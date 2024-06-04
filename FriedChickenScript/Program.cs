using FriedChickenScript;

class Program
{
    static void Main(string[] args)
    {
        //string filePath = "testScript.fc";
        string script = File.ReadAllText(args[0]);

        Lexer lexer = new Lexer(script);
        List<Token> tokens = lexer.Tokenise();
        Console.WriteLine("Processing...");
        Console.WriteLine("-----------------------------------------");
        tokens.ForEach(t => Console.WriteLine(t.ToString()));

        Console.WriteLine("-----------------------------------------");
        Parser parser = new Parser(tokens);
        ASTNode ASTRoot = parser.Parse();

        PrintTree(ASTRoot);
        Console.WriteLine("-----------------------------------------");
        Interpreter interpreter = new Interpreter();
        interpreter.Interpret(ASTRoot);
        interpreter.PrintVariables();
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