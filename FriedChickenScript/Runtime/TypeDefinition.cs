namespace FriedChickenScript;

// A `bucket` type: an ordered list of fields, each with an optional default expression
// that is evaluated fresh whenever an instance is created, plus recipes (methods) shared
// by every instance and invoked as `instance.method(...)`.
public class TypeDefinition
{
    public string Name { get; }
    public List<FieldDefinition> Fields { get; }
    public Dictionary<string, Function> Methods { get; }

    public TypeDefinition(string name, List<FieldDefinition> fields, Dictionary<string, Function> methods)
    {
        Name = name;
        Fields = fields;
        Methods = methods;
    }
}

public class FieldDefinition
{
    public string Name { get; }
    public ASTNode? Default { get; } // null => defaults to EMPTY (null)

    public FieldDefinition(string name, ASTNode? defaultExpr)
    {
        Name = name;
        Default = defaultExpr;
    }
}
