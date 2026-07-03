namespace FriedChickenScript;

// Tree-walking interpreter. Executes statements against an Environment (scope chain);
// expression values are produced by ExpressionEvaluator. Return values propagate via
// ReturnException, so a `serve` deep inside nested blocks unwinds straight to the call.
public class Interpreter
{
    private readonly Environment globals = new();
    private readonly Dictionary<string, Function> functions = new();
    private readonly Dictionary<string, TypeDefinition> types = new();
    private readonly ExpressionEvaluator evaluator;

    public Interpreter()
    {
        evaluator = new ExpressionEvaluator(this);
    }

    public void Run(ASTNode program)
    {
        // Hoist recipe/bucket declarations first so calls and instantiations can appear
        // before the definition they refer to.
        foreach (var child in program.Children)
        {
            if (child.Type == NodeType.FunctionDeclaration)
            {
                RegisterFunction(child);
            }
            else if (child.Type == NodeType.BucketDeclaration)
            {
                RegisterType(child);
            }
        }

        foreach (var child in program.Children)
        {
            Execute(child, globals);
        }
    }

    public void Execute(ASTNode node, Environment env)
    {
        switch (node.Type)
        {
            case NodeType.Program:
                foreach (var child in node.Children)
                {
                    Execute(child, env);
                }
                break;

            case NodeType.Block:
                Environment blockScope = new Environment(env);
                foreach (var child in node.Children)
                {
                    Execute(child, blockScope);
                }
                break;

            case NodeType.VariableDeclaration:
                if (env.ExistsLocally(node.Value!))
                    throw new FcRuntimeException($"'{node.Value}' is already defined in this scope");
                env.Define(node.Value!, Evaluate(node.Children[0], env));
                break;

            case NodeType.Assignment:
                env.Assign(node.Value!, Evaluate(node.Children[0], env));
                break;

            case NodeType.MemberAssignment:
                ExecuteMemberAssignment(node, env);
                break;

            case NodeType.FunctionDeclaration:
                RegisterFunction(node);
                break;

            case NodeType.BucketDeclaration:
                RegisterType(node);
                break;

            case NodeType.ObjectInstantiation:
                ExecuteInstantiation(node, env);
                break;

            case NodeType.FunctionCall:
                Evaluate(node, env); // called for its effect; return value discarded
                break;

            case NodeType.ReturnStatement:
                throw new ReturnException(Evaluate(node.Children[0], env));

            case NodeType.PrintStatement:
                Console.WriteLine(ValueOps.Stringify(Evaluate(node.Children[0], env)));
                break;

            case NodeType.IfStatement:
                if (ValueOps.Truthy(Evaluate(node.Children[0], env)))
                    Execute(node.Children[1], env);
                else if (node.Children.Count > 2)
                    Execute(node.Children[2], env);
                break;

            case NodeType.WhileStatement:
                while (ValueOps.Truthy(Evaluate(node.Children[0], env)))
                {
                    Execute(node.Children[1], env);
                }
                break;

            default:
                throw new FcRuntimeException($"Unhandled statement: {node.Type}");
        }
    }

    public object? Evaluate(ASTNode node, Environment env) => evaluator.Evaluate(node, env);

    // Called from ExpressionEvaluator for both statement and expression calls.
    public object? CallFunction(string name, List<object?> args)
    {
        if (!functions.TryGetValue(name, out var function))
            throw new FcRuntimeException($"Recipe '{name}' is not defined");
        if (function.Parameters.Count != args.Count)
            throw new FcRuntimeException(
                $"Recipe '{name}' expects {function.Parameters.Count} argument(s) but got {args.Count}");

        // Functions are top-level, so they close over globals (not the caller's locals).
        Environment callEnv = new Environment(globals);
        for (int i = 0; i < function.Parameters.Count; i++)
        {
            callEnv.Define(function.Parameters[i], args[i]);
        }

        try
        {
            Execute(function.Body, callEnv);
        }
        catch (ReturnException r)
        {
            return r.Value;
        }
        return null;
    }

    private void ExecuteInstantiation(ASTNode node, Environment env)
    {
        string typeName = node.Value!;
        string varName = node.Children[0].Value!;

        if (!types.TryGetValue(typeName, out var type))
            throw new FcRuntimeException($"Unknown bucket type '{typeName}'");

        object? value;
        if (node.Children.Count > 1)
        {
            value = Evaluate(node.Children[1], env);
            if (value is not FcObject fo)
                throw new FcRuntimeException($"Cannot initialise '{varName}' of type {typeName} from a non-object value");
            if (fo.TypeName != typeName)
                throw new FcRuntimeException($"Type mismatch: '{varName}' is {typeName} but got {fo.TypeName}");
        }
        else
        {
            var instance = new FcObject(typeName);
            foreach (var field in type.Fields)
            {
                instance.Fields[field.Name] = field.Default != null ? Evaluate(field.Default, env) : null;
            }
            value = instance;
        }

        env.Define(varName, value);
    }

    private void ExecuteMemberAssignment(ASTNode node, Environment env)
    {
        object? target = Evaluate(node.Children[0], env);
        if (target is not FcObject obj)
            throw new FcRuntimeException($"Cannot set field '{node.Value}' on a non-object value");
        if (!obj.Fields.ContainsKey(node.Value!))
            throw new FcRuntimeException($"'{obj.TypeName}' has no field '{node.Value}'");
        obj.Fields[node.Value!] = Evaluate(node.Children[1], env);
    }

    private void RegisterFunction(ASTNode node)
    {
        var parameters = node.Children
            .Where(c => c.Type == NodeType.Parameter)
            .Select(c => c.Value!)
            .ToList();
        var body = node.Children.First(c => c.Type == NodeType.Block);
        functions[node.Value!] = new Function(node.Value!, parameters, body);
    }

    private void RegisterType(ASTNode node)
    {
        var fields = node.Children
            .Select(f => new FieldDefinition(f.Value!, f.Children.Count > 0 ? f.Children[0] : null))
            .ToList();
        types[node.Value!] = new TypeDefinition(node.Value!, fields);
    }
}
