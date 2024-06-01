namespace FriedChickenScript;

public enum NodeType
{
    Program,
    FunctionDeclaration,
    Parameter,
    Arguments,
    Block,
    ReturnStatement,
    BinaryExpression,
    VariableDeclaration,
    Assignment,
    FunctionCall,
    PrintStatement,
    Identifier,
    Literal,
}

public class ASTNode
{
    public NodeType Type { get; }
    public string Value { get; }
    public List<ASTNode> Children { get; }

    public ASTNode(NodeType type, string value = null)
    {
        Type = type;
        Value = value;
        Children = new List<ASTNode>();
    }

    public void AddChild(ASTNode child)
    {
        Children.Add(child);
    }

    public override string ToString()
    {
        return $"{Type}({Value})";
    }
}
