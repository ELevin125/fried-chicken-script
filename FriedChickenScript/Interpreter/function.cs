namespace FriedChickenScript;

public class Function
{
    public string Name { get; }
    public List<string> Parameters { get; }
    public ASTNode Body { get; }

    public Function(string name, List<string> parameters, ASTNode body)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
    }
}