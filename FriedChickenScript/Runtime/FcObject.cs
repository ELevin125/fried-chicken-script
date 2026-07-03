namespace FriedChickenScript;

// A runtime instance of a bucket type. First-class value: it can be stored in an
// Environment, passed to a recipe, and returned via serve, just like any other value.
public class FcObject
{
    public string TypeName { get; }
    public Dictionary<string, object?> Fields { get; }

    public FcObject(string typeName, Dictionary<string, object?>? fields = null)
    {
        TypeName = typeName;
        Fields = fields ?? new Dictionary<string, object?>();
    }

    public override string ToString()
    {
        var body = string.Join(", ", Fields.Select(f => $"{f.Key}={ValueOps.Stringify(f.Value)}"));
        return $"{TypeName} {{ {body} }}";
    }
}
