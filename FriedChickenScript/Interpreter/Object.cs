namespace FriedChickenScript;

public class Object
{
    public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
    public Dictionary<string, Function> Methods { get; } = new Dictionary<string, Function>();

    public Object (Dictionary<string, object> properties = null, Dictionary<string, Function> methods = null)
    {
        Properties = properties;
        Methods = methods;
    }
}