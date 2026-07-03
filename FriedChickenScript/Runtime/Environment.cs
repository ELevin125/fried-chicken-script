namespace FriedChickenScript;

// A single lexical scope in a chain. Variable lookups walk up to the parent, but
// definitions always land in the current scope, so locals declared inside a block or
// recipe body vanish when that scope ends and can never leak into the parent.
//
// (Named Environment to match the interpreter design; within this namespace it shadows
// System.Environment, which we never use here.)
public class Environment
{
    private readonly Environment? parent;
    private readonly Dictionary<string, object?> values = new();

    public Environment(Environment? parent = null)
    {
        this.parent = parent;
    }

    // Introduce a new binding in this scope (shadowing any same-named binding above).
    public void Define(string name, object? value)
    {
        values[name] = value;
    }

    // Read a binding, searching this scope then outward. Throws if never defined.
    public object? Get(string name)
    {
        if (values.TryGetValue(name, out var value))
            return value;
        if (parent != null)
            return parent.Get(name);
        throw new FcRuntimeException($"'{name}' is not defined");
    }

    // Update an existing binding wherever it lives in the chain. Throws if it was never
    // declared (assignment must target an existing variable).
    public void Assign(string name, object? value)
    {
        if (values.ContainsKey(name))
        {
            values[name] = value;
            return;
        }
        if (parent != null)
        {
            parent.Assign(name, value);
            return;
        }
        throw new FcRuntimeException($"'{name}' does not exist in the current context");
    }

    public bool ExistsLocally(string name) => values.ContainsKey(name);
}
