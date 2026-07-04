using System.Globalization;

namespace FriedChickenScript;

// Evaluates expression nodes to runtime values (int / double / string / bool / null / FcObject).
// Statement execution and function calls are delegated back to the Interpreter.
public class ExpressionEvaluator
{
    private readonly Interpreter interpreter;

    public ExpressionEvaluator(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    public object? Evaluate(ASTNode node, Environment env)
    {
        switch (node.Type)
        {
            case NodeType.NumberLiteral:
                return ParseNumber(node.Value!);

            case NodeType.StringLiteral:
                return node.Value;

            case NodeType.BooleanLiteral:
                return node.Value == "true";

            case NodeType.NullLiteral:
                return null;

            case NodeType.Identifier:
                return env.Get(node.Value!);

            case NodeType.MemberAccess:
                return EvaluateMemberAccess(node, env);

            case NodeType.ArrayLiteral:
                return node.Children.Select(c => Evaluate(c, env)).ToList();

            case NodeType.IndexAccess:
                return EvaluateIndexAccess(node, env);

            case NodeType.MethodCall:
                return EvaluateMethodCall(node, env);

            case NodeType.FunctionCall:
                return EvaluateCall(node, env);

            case NodeType.UnaryExpression:
                return EvaluateUnary(node, env);

            case NodeType.BinaryExpression:
                return EvaluateBinary(node, env);

            default:
                throw new FcRuntimeException($"Cannot evaluate node: {node.Type}");
        }
    }

    private object? EvaluateMemberAccess(ASTNode node, Environment env)
    {
        object? target = Evaluate(node.Children[0], env);

        if (target is List<object?> list)
        {
            if (node.Value == "length")
            {
                return list.Count;
            }
            throw new FcRuntimeException($"Lists have no member '{node.Value}' (try .length)");
        }

        if (target is not FcObject obj)
        {
            throw new FcRuntimeException($"Cannot read field '{node.Value}' from a non-object value");
        }
        if (!obj.Fields.TryGetValue(node.Value!, out var value))
        {
            throw new FcRuntimeException($"'{obj.TypeName}' has no field '{node.Value}'");
        }
        return value;
    }

    private object? EvaluateIndexAccess(ASTNode node, Environment env)
    {
        List<object?> list = AsList(Evaluate(node.Children[0], env));
        int index = AsIndex(Evaluate(node.Children[1], env), list.Count);
        return list[index];
    }

    private object? EvaluateMethodCall(ASTNode node, Environment env)
    {
        object? target = Evaluate(node.Children[0], env);
        ASTNode? argsNode = node.Children.FirstOrDefault(c => c.Type == NodeType.Arguments);
        List<object?> args = argsNode == null
            ? new List<object?>()
            : argsNode.Children.Select(a => Evaluate(a, env)).ToList();

        if (target is List<object?> list)
        {
            return CallListMethod(list, node.Value!, args);
        }
        if (target is FcObject obj)
        {
            return interpreter.CallMethod(obj, node.Value!, args);
        }
        throw new FcRuntimeException($"Value has no method '{node.Value}'");
    }

    private static object? CallListMethod(List<object?> list, string method, List<object?> args)
    {
        switch (method)
        {
            case "length":
                RequireArgs(method, args, 0);
                return list.Count;

            case "add":
                RequireArgs(method, args, 1);
                list.Add(args[0]);
                return null;

            case "remove":
                RequireArgs(method, args, 1);
                int index = AsIndex(args[0], list.Count);
                object? removed = list[index];
                list.RemoveAt(index);
                return removed;

            default:
                throw new FcRuntimeException($"Lists have no method '{method}' (try add, remove, length)");
        }
    }

    // Shared list-access guards, also used by the interpreter's index-assignment.
    public static List<object?> AsList(object? value)
    {
        if (value is List<object?> list)
        {
            return list;
        }
        throw new FcRuntimeException("Value is not a list");
    }

    public static int AsIndex(object? value, int count)
    {
        if (value is not int index)
        {
            throw new FcRuntimeException("List index must be a whole number");
        }
        if (index < 0 || index >= count)
        {
            throw new FcRuntimeException($"Index {index} is out of range (list length {count})");
        }
        return index;
    }

    private static void RequireArgs(string method, List<object?> args, int expected)
    {
        if (args.Count != expected)
        {
            throw new FcRuntimeException($"'{method}' expects {expected} argument(s) but got {args.Count}");
        }
    }

    private object? EvaluateCall(ASTNode node, Environment env)
    {
        var argsNode = node.Children.FirstOrDefault(c => c.Type == NodeType.Arguments);
        var args = argsNode == null
            ? new List<object?>()
            : argsNode.Children.Select(a => Evaluate(a, env)).ToList();
        return interpreter.CallFunction(node.Value!, args);
    }

    private object? EvaluateUnary(ASTNode node, Environment env)
    {
        object? operand = Evaluate(node.Children[0], env);
        switch (node.Value)
        {
            case Syntax.Not:
                // Logical negation over truthiness (RAW / EMPTY / 0 / "" are false).
                return !ValueOps.Truthy(operand);
            case Syntax.Subtraction:
                return ValueOps.Negate(operand);
            default:
                throw new FcRuntimeException($"Unknown unary operator '{node.Value}'");
        }
    }

    private object? EvaluateBinary(ASTNode node, Environment env)
    {
        string op = node.Value!;

        // Short-circuit the logical operators (don't evaluate the right side unless needed).
        if (op == Syntax.And)
            return ValueOps.Truthy(Evaluate(node.Children[0], env)) && ValueOps.Truthy(Evaluate(node.Children[1], env));
        if (op == Syntax.Or)
            return ValueOps.Truthy(Evaluate(node.Children[0], env)) || ValueOps.Truthy(Evaluate(node.Children[1], env));

        object? left = Evaluate(node.Children[0], env);
        object? right = Evaluate(node.Children[1], env);
        return ValueOps.Apply(op, left, right);
    }

    // A literal with a decimal point is a double; otherwise an int
    private static object ParseNumber(string text)
    {
        if (text.Contains('.'))
        {
            return double.Parse(text, CultureInfo.InvariantCulture);
        }
        return int.Parse(text);
    }
}
