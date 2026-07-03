namespace FriedChickenScript;

public enum NodeType
{
    Program,
    Block,

    // Variables
    VariableDeclaration,   // Value = name;        child[0] = initializer expression
    Assignment,            // Value = name;        child[0] = value expression

    // Functions
    FunctionDeclaration,   // Value = name;        Parameter children + one Block child
    Parameter,             // Value = parameter name
    Arguments,             // children = argument expressions
    FunctionCall,          // Value = name;        optional Arguments child
    ReturnStatement,       // child[0] = expression

    // Output
    PrintStatement,        // child[0] = expression

    // Control flow
    IfStatement,           // child[0] = condition, child[1] = then-Block, [child[2] = else-Block]
    WhileStatement,        // child[0] = condition, child[1] = body-Block

    // Objects (buckets)
    BucketDeclaration,     // Value = type name;   FieldDeclaration children
    FieldDeclaration,      // Value = field name;  optional child[0] = default expression
    ObjectInstantiation,   // Value = type name;   child[0] = Identifier(varName), [child[1] = initializer]
    MemberAccess,          // Value = field name;  child[0] = target expression
    MemberAssignment,      // Value = field name;  child[0] = target expr, child[1] = value expr

    // Expressions
    BinaryExpression,      // Value = operator;    child[0] = left, child[1] = right
    Identifier,            // Value = name

    // Literals
    NumberLiteral,         // Value = digits
    StringLiteral,         // Value = text
    BooleanLiteral,        // Value = "true" | "false"
    NullLiteral
}

public class ASTNode
{
    public NodeType Type { get; }
    public string? Value { get; }
    public List<ASTNode> Children { get; }

    public ASTNode(NodeType type, string? value = null)
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
        return Value == null ? $"{Type}" : $"{Type}({Value})";
    }
}
