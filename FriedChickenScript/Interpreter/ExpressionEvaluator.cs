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
        if (target is not FcObject obj)
            throw new FcRuntimeException($"Cannot read field '{node.Value}' from a non-object value");
        if (!obj.Fields.TryGetValue(node.Value!, out var value))
            throw new FcRuntimeException($"'{obj.TypeName}' has no field '{node.Value}'");
        return value;
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
