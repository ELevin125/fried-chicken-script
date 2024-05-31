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
    }
}