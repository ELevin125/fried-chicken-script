namespace FriedChickenScript;

public class Interpreter
{
    private Dictionary<string, object> variables = new Dictionary<string, object>();

    public Interpreter() { }

    public void Interpret(ASTNode node)
    {
        switch (node.Type)
        {
            case NodeType.Program:
                foreach (var child in node.Children)
                {
                    Interpret(child);
                }
                break;
            case NodeType.VariableDeclaration:
                if (variables.ContainsKey(node.Value))
                    throw new InvalidOperationException($"The variable '{node.Value}' is already defined");

                variables[node.Value] = InterpretExpression(node.Children.First()); ;
                break;
            case NodeType.Assignment:
                if (!variables.ContainsKey(node.Value))
                    throw new InvalidOperationException($"The variable '{node.Value}' does not exist in the current context");

                variables[node.Value] = InterpretExpression(node.Children.First()); ;
                break;
        }

    }

    public void PrintVariables()
    {
        foreach (var kvp in variables)
        {
            Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
        }
    }

    private object InterpretExpression(ASTNode node)
    {
        switch (node.Type)
        {
            case NodeType.Literal:
                if (int.TryParse(node.Value, out int intValue))
                {
                    return intValue;
                }
                return node.Value;

            case NodeType.Identifier:
                return variables[node.Value];

            case NodeType.BinaryExpression:
                var left = InterpretExpression(node.Children[0]);
                var right = InterpretExpression(node.Children[1]);
                string operatorType = node.Value;

                switch (operatorType)
                {
                    // Math
                    case Syntax.Addition:
                        return (int)left + (int)right;
                    case Syntax.Subtraction:
                        return (int)left - (int)right;
                    case Syntax.Multiplication:
                        return (int)left * (int)right;
                    case Syntax.Division:
                        return (int)left / (int)right;

                    // Logic
                    case Syntax.Equality:
                        return (int)left == (int)right;
                    case Syntax.Inequality:
                        return (int)left != (int)right;
                    case Syntax.LessThan:
                        return (int)left < (int)right;
                    case Syntax.GreaterThan:
                        return (int)left > (int)right;
                    case Syntax.EqLessThan:
                        return (int)left <= (int)right;
                    case Syntax.EqGreaterThan:
                        return (int)left >= (int)right;
                    default:
                        throw new InvalidOperationException($"Unrecognized operator: {operatorType}");
                }
        }

        return null;
    }
}

