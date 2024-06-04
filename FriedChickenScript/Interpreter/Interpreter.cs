using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FriedChickenScript;

public class Interpreter
{
    private Dictionary<string, object> variables = new Dictionary<string, object>();
    private Dictionary<string, Function> functions = new Dictionary<string, Function>();

    public Interpreter() { }

    public object Interpret(ASTNode node)
    {
        object returnedVal;
        switch (node.Type)
        {
            case NodeType.Program:
            case NodeType.Block:
                foreach (var child in node.Children)
                {
                    object value = Interpret(child);
                    if (value != null)
                        return value;
                }
                break;

            case NodeType.VariableDeclaration:
                if (variables.ContainsKey(node.Value))
                    throw new InvalidOperationException($"The variable '{node.Value}' is already defined");

                returnedVal = InterpretExpression(node.Children.First());
                variables[node.Value] = returnedVal;
                break;

            case NodeType.Assignment:
                if (!variables.ContainsKey(node.Value))
                    throw new InvalidOperationException($"The variable '{node.Value}' does not exist in the current context");

                returnedVal = InterpretExpression(node.Children.First());
                variables[node.Value] = returnedVal;
                break;

            case NodeType.FunctionDeclaration:
                InterpretFunction(node);
                break;

            case NodeType.FunctionCall:
                // This function call does not trigger when assigning a value to a variable
                // only when called standalone 
                ExecuteFunction(node);
                break;

            case NodeType.ReturnStatement:
                returnedVal = InterpretExpression(node.Children.First());
                return returnedVal;

            case NodeType.IfStatement:
                returnedVal = InterpretIfStatement(node);
                return returnedVal;

            case NodeType.WhileStatement:
                returnedVal = InterpretWhileStatement(node);
                return returnedVal;

            default:
                throw new InvalidOperationException($"Unhandled node type: {node.Type}");
        }

        return null;
    }

    public void PrintVariables()
    {
        foreach (var kvp in variables)
        {
            Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
        }
    }

    private object InterpretIfStatement(ASTNode node)
    {

        bool condition = (bool)InterpretExpression(node.Children[0]);
        if (condition)
        {
            var ifBody = node.Children[1];
            var value = ExecuteBlock(ifBody, variables);
            return value;
        }
        else if (node.Children.Count > 2)
        {
            var elseBody = node.Children[2];
            var value = ExecuteBlock(elseBody, variables);
            return value;
        }
        return null;
    }
    
    private object InterpretWhileStatement(ASTNode node)
    {

        bool condition = (bool)InterpretExpression(node.Children[0]);
        if (condition)
        {
            var body = node.Children[1];
            var value = ExecuteBlock(body, variables);

            if (value != null)
                return value;
            else
                return InterpretWhileStatement(node);
        }
        return null;
    }

    private void InterpretFunction(ASTNode node)
    {
        string functionName = node.Value;
        var parameters = node.Children.Where(c => c.Type == NodeType.Parameter).Select(p => p.Value).ToList();
        var body = node.Children.First(c => c.Type == NodeType.Block);
        Function f = new Function(functionName, parameters, body);
        functions[functionName] = f;
    }

    private object ExecuteFunction(ASTNode node)
    {
        string functionName = node.Value;
        if (!functions.ContainsKey(functionName))
            throw new InvalidOperationException($"Function '{functionName}' is not defined");

        Function function = functions[functionName];
        List<object> arguments = node.Children
            .Where(c => c.Type == NodeType.Arguments)
            .SelectMany(argNode => argNode.Children.Select(arg => InterpretExpression(arg))) // Interpret all arguments
            .ToList();

        if (function.Parameters.Count != arguments.Count)
            throw new InvalidOperationException($"Function '{functionName}' expects {function.Parameters.Count} arguments, but {arguments.Count} were provided");

        // Create a new scope for the function body that will include the parameters as variables
        var scopedVariables = new Dictionary<string, object>(variables);
        for (int i = 0; i < function.Parameters.Count; i++)
        {
            scopedVariables[function.Parameters[i]] = arguments[i];
        }

        object value = ExecuteBlock(function.Body, scopedVariables);
        return value;
    }

    // Interpret the block node with the scopeVariables provided
    // After execution, the variables defined in this block are removed, however
    // updates to existing variables are kept
    private object ExecuteBlock(ASTNode node, Dictionary<string, object> scopeVariables)
    {
        var previousVariables = new Dictionary<string, object>(variables);
        variables = scopeVariables;

        object returnVal = Interpret(node);

        // Restore previous variable scope after function execution
        // Only the values for variables defined before the block are kept
        var mergedDict = previousVariables.Keys.ToDictionary(k => k, k => variables.ContainsKey(k) ? variables[k] : previousVariables[k]);
        variables = mergedDict;

        return returnVal;
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
                else if (bool.TryParse(node.Value, out bool boolValue))
                {
                    return boolValue;
                }
                return node.Value;

            case NodeType.Identifier:
                return variables[node.Value];
            
            case NodeType.FunctionCall:
                return ExecuteFunction(node);

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
                        return left?.ToString() == right?.ToString();
                    case Syntax.Inequality:
                        return left?.ToString() != right?.ToString();
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

