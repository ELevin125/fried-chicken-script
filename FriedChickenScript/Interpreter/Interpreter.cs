namespace FriedChickenScript;

// Tree-walking interpreter. Executes statements against an Environment (scope chain);
// expression values are produced by ExpressionEvaluator. Return values propagate via
// ReturnException, so a `serve` deep inside nested blocks unwinds straight to the call.
public class Interpreter
{
    private readonly Environment globals = new();
    private readonly Dictionary<string, Function> functions = new();
    private readonly Dictionary<string, TypeDefinition> types = new();
    private readonly Dictionary<string, Func<List<object?>, object?>> builtins;
    private readonly ExpressionEvaluator evaluator;

    // Source of randomness for the `random` builtin. Time-seeded by default (so each run
    // differs); `randomSeed` swaps in a fixed seed for reproducible runs.
    private Random rng = new();

    public Interpreter()
    {
        evaluator = new ExpressionEvaluator(this);
        builtins = new Dictionary<string, Func<List<object?>, object?>>
        {
            [Syntax.Print] = BuiltinPrint,
            [Syntax.ReadIO] = BuiltinReadInput,
            [Syntax.Random] = BuiltinRandom,
            [Syntax.RandomSeed] = BuiltinRandomSeed,
        };
    }

    public void Run(ASTNode program)
    {
        Hoist(program);

        try
        {
            foreach (var child in program.Children)
            {
                Execute(child, globals);
            }
        }
        catch (BreakException)
        {
            throw BreakOutsideLoop();
        }
    }

    // Run one fragment typed at the REPL against the persistent global scope. Statements
    // execute for their effect; a bare expression is evaluated and its value returned so
    // the shell can echo it. Returns the last expression value, or null.
    public object? RunRepl(ASTNode program)
    {
        Hoist(program);

        object? last = null;
        try
        {
            foreach (var child in program.Children)
            {
                if (IsExpression(child))
                {
                    last = Evaluate(child, globals);
                }
                else
                {
                    Execute(child, globals);
                    last = null;
                }
            }
        }
        catch (BreakException)
        {
            throw BreakOutsideLoop();
        }
        return last;
    }

    // Names currently defined in the session (for the REPL's .vars command).
    public IReadOnlyCollection<string> DefinedVariables => globals.Names();
    public IReadOnlyCollection<string> DefinedRecipes => functions.Keys.ToList();
    public IReadOnlyCollection<string> DefinedBuckets => types.Keys.ToList();

    // Register recipe/bucket declarations first so calls and instantiations can appear
    // before the definition they refer to.
    private void Hoist(ASTNode program)
    {
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
    }

    private static bool IsExpression(ASTNode node)
    {
        switch (node.Type)
        {
            case NodeType.NumberLiteral:
            case NodeType.StringLiteral:
            case NodeType.BooleanLiteral:
            case NodeType.NullLiteral:
            case NodeType.Identifier:
            case NodeType.MemberAccess:
            case NodeType.FunctionCall:
            case NodeType.BinaryExpression:
            case NodeType.UnaryExpression:
            case NodeType.ArrayLiteral:
            case NodeType.IndexAccess:
            case NodeType.MethodCall:
                return true;
            default:
                return false;
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
                EnsureNotBuiltin(node.Value!);
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

            case NodeType.IndexAssignment:
                ExecuteIndexAssignment(node, env);
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

            case NodeType.MethodCall:
                Evaluate(node, env); // e.g. parts.add(x) used as a statement
                break;

            case NodeType.ReturnStatement:
                throw new ReturnException(Evaluate(node.Children[0], env));

            case NodeType.IfStatement:
                if (ValueOps.Truthy(Evaluate(node.Children[0], env)))
                    Execute(node.Children[1], env);
                else if (node.Children.Count > 2)
                    Execute(node.Children[2], env);
                break;

            case NodeType.WhileStatement:
                try
                {
                    while (ValueOps.Truthy(Evaluate(node.Children[0], env)))
                    {
                        Execute(node.Children[1], env);
                    }
                }
                catch (BreakException)
                {
                    // closeShop: leave the loop early.
                }
                break;

            case NodeType.BreakStatement:
                throw new BreakException();

            default:
                throw new FcRuntimeException($"Unhandled statement: {node.Type}");
        }
    }

    public object? Evaluate(ASTNode node, Environment env) => evaluator.Evaluate(node, env);

    // Called from ExpressionEvaluator for both statement and expression calls.
    public object? CallFunction(string name, List<object?> args)
    {
        if (builtins.TryGetValue(name, out var builtin))
            return builtin(args);
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
        catch (BreakException)
        {
            throw BreakOutsideLoop();
        }
        return null;
    }

    // Called from ExpressionEvaluator for `instance.method(args)`. The instance is bound to
    // the `myBucket` receiver so the body can read and write its own fields.
    public object? CallMethod(FcObject instance, string methodName, List<object?> args)
    {
        if (!types.TryGetValue(instance.TypeName, out var type))
            throw new FcRuntimeException($"Unknown bucket type '{instance.TypeName}'");
        if (!type.Methods.TryGetValue(methodName, out var method))
            throw new FcRuntimeException($"'{instance.TypeName}' has no recipe '{methodName}'");
        if (method.Parameters.Count != args.Count)
            throw new FcRuntimeException(
                $"Recipe '{methodName}' expects {method.Parameters.Count} argument(s) but got {args.Count}");

        // Like top-level recipes, methods close over globals; additionally the receiver and
        // the arguments are bound in the call scope.
        Environment callEnv = new Environment(globals);
        callEnv.Define(Syntax.Self, instance);
        for (int i = 0; i < method.Parameters.Count; i++)
        {
            callEnv.Define(method.Parameters[i], args[i]);
        }

        try
        {
            Execute(method.Body, callEnv);
        }
        catch (ReturnException r)
        {
            return r.Value;
        }
        catch (BreakException)
        {
            throw BreakOutsideLoop();
        }
        return null;
    }

    // A closeShop that escapes to a recipe-call boundary or top level was used outside any
    // loop; surface it as a clear runtime error rather than letting the signal leak out.
    private static FcRuntimeException BreakOutsideLoop() =>
        new FcRuntimeException($"'{Syntax.Break}' can only be used inside a {Syntax.While} loop");

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

    private void ExecuteIndexAssignment(ASTNode node, Environment env)
    {
        List<object?> list = ExpressionEvaluator.AsList(Evaluate(node.Children[0], env));
        int index = ExpressionEvaluator.AsIndex(Evaluate(node.Children[1], env), list.Count);
        list[index] = Evaluate(node.Children[2], env);
    }

    private void RegisterFunction(ASTNode node)
    {
        EnsureNotBuiltin(node.Value!);
        functions[node.Value!] = BuildFunction(node);
    }

    // Builtins are reserved: a program can't declare a recipe or variable that shadows one,
    // since the shadow would be silently ignored at call time.
    private void EnsureNotBuiltin(string name)
    {
        if (builtins.ContainsKey(name))
        {
            throw new FcRuntimeException($"'{name}' is a builtin and cannot be redefined");
        }
    }

    private void RegisterType(ASTNode node)
    {
        var fields = node.Children
            .Where(c => c.Type == NodeType.FieldDeclaration)
            .Select(f => new FieldDefinition(f.Value!, f.Children.Count > 0 ? f.Children[0] : null))
            .ToList();
        var methods = node.Children
            .Where(c => c.Type == NodeType.FunctionDeclaration)
            .ToDictionary(m => m.Value!, BuildFunction);
        types[node.Value!] = new TypeDefinition(node.Value!, fields, methods);
    }

    // Build a Function from a FunctionDeclaration node — shared by top-level recipes and
    // bucket methods, which parse identically.
    private static Function BuildFunction(ASTNode node)
    {
        var parameters = node.Children
            .Where(c => c.Type == NodeType.Parameter)
            .Select(c => c.Value!)
            .ToList();
        var body = node.Children.First(c => c.Type == NodeType.Block);
        return new Function(node.Value!, parameters, body);
    }

    // orderUp(value) -> print one value on its own line; returns EMPTY.
    private object? BuiltinPrint(List<object?> args)
    {
        if (args.Count != 1)
        {
            throw new FcRuntimeException($"'{Syntax.Print}' expects 1 argument but got {args.Count}");
        }
        Console.WriteLine(ValueOps.Stringify(args[0]));
        return null;
    }

    // takeOrder() -> read one line of input, or "" at end of input.
    private object? BuiltinReadInput(List<object?> args)
    {
        if (args.Count != 0)
        {
            throw new FcRuntimeException($"'{Syntax.ReadIO}' expects no arguments but got {args.Count}");
        }
        return Console.ReadLine() ?? "";
    }

    // random()  -> a double in [0, 1)
    // random(n) -> a whole number in [0, n), where n is a positive whole number
    private object? BuiltinRandom(List<object?> args)
    {
        if (args.Count == 0)
        {
            return rng.NextDouble();
        }
        if (args.Count == 1)
        {
            int bound = AsWholeNumber(args[0], "random");
            if (bound <= 0)
            {
                throw new FcRuntimeException("'random' needs a positive whole number, e.g. random(6)");
            }
            return rng.Next(bound);
        }
        throw new FcRuntimeException($"'random' expects 0 or 1 argument(s) but got {args.Count}");
    }

    // randomSeed(n) -> reseed the generator so a run is reproducible; returns EMPTY.
    private object? BuiltinRandomSeed(List<object?> args)
    {
        if (args.Count != 1)
        {
            throw new FcRuntimeException($"'randomSeed' expects 1 argument but got {args.Count}");
        }
        rng = new Random(AsWholeNumber(args[0], "randomSeed"));
        return null;
    }

    // Accept an int, or a double that has no fractional part; reject anything else so a
    // stray string or object surfaces a clear error rather than a silent truncation.
    private static int AsWholeNumber(object? value, string fn)
    {
        if (value is int i)
        {
            return i;
        }
        if (value is double d && d == Math.Floor(d))
        {
            return (int)d;
        }
        throw new FcRuntimeException($"'{fn}' expects a whole number");
    }
}
