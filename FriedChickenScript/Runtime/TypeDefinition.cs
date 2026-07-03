namespace FriedChickenScript;

// A `bucket` type: an ordered list of fields, each with an optional default expression
// that is evaluated fresh whenever an instance is created.
public class TypeDefinition
{
    public string Name { get; }
    public List<FieldDefinition> Fields { get; }

    public TypeDefinition(string name, List<FieldDefinition> fields)
    {
        Name = name;
        Fields = fields;
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
