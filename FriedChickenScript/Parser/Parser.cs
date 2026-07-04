namespace FriedChickenScript;

public class Parser
{
    public List<Token> Tokens { get; }
    public int TokenIndex { get; private set; } = 0;

    public Parser(List<Token> tokens)
    {
        Tokens = tokens;
    }

    public ASTNode Parse()
    {
        ASTNode root = new ASTNode(NodeType.Program);
        while (TokenIndex < Tokens.Count)
        {
            root.AddChild(ParseStatement());
        }
        return root;
    }

    public ASTNode ParseStatement()
    {
        Token token = Require();

        if (token.Type == TokenType.Keyword)
        {
            switch (token.Value)
            {
                case Syntax.Variable: return VariableParser.ParseDeclaration(this);
                case Syntax.Object:   return ObjectParser.ParseTypeDefinition(this);
                case Syntax.Function: return FunctionParser.ParseDeclaration(this);
                case Syntax.Return:   return FunctionParser.ParseReturn(this);
                case Syntax.If:       return MiscParser.ParseIfStatement(this);
                case Syntax.While:    return MiscParser.ParseWhileStatement(this);
                case Syntax.Print:    return MiscParser.ParsePrintStatement(this);
            }
        }

        // `TypeName varName [= expr]` — an object instantiation (two identifiers in a row).
        if (token.Type == TokenType.Identifier && Peek(1)?.Type == TokenType.Identifier)
            return ObjectParser.ParseInstantiation(this);

        // Otherwise it's an expression that may be an assignment target.
        ASTNode expr = ExpressionParser.ParseExpression(this);
        if (GetCurrentToken()?.Type == TokenType.Operator && GetCurrentToken()?.Value == Syntax.Assignment)
        {
            Consume(TokenType.Operator, Syntax.Assignment);
            ASTNode value = ExpressionParser.ParseExpression(this);
            return MakeAssignment(expr, value);
        }

        return expr;
    }

    private static ASTNode MakeAssignment(ASTNode target, ASTNode value)
    {
        switch (target.Type)
        {
            case NodeType.Identifier:
                ASTNode assign = new ASTNode(NodeType.Assignment, target.Value);
                assign.AddChild(value);
                return assign;

            case NodeType.MemberAccess:
                ASTNode member = new ASTNode(NodeType.MemberAssignment, target.Value);
                member.AddChild(target.Children[0]); // the object being assigned into
                member.AddChild(value);
                return member;

            case NodeType.IndexAccess:
                ASTNode indexed = new ASTNode(NodeType.IndexAssignment);
                indexed.AddChild(target.Children[0]); // the list
                indexed.AddChild(target.Children[1]); // the index
                indexed.AddChild(value);
                return indexed;

            default:
                throw new FcParseException($"Invalid assignment target: {target.Type}");
        }
    }

    public Token Consume(TokenType type, string? value = null)
    {
        Token token = Require();
        if (token.Type != type || (value != null && token.Value != value))
            throw new FcParseException(
                $"Expected {type}{(value != null ? $" '{value}'" : "")}, but got {token.Type} '{token.Value}' at line {token.Line}");
        TokenIndex++;
        return token;
    }

    public Token? GetCurrentToken() => TokenIndex < Tokens.Count ? Tokens[TokenIndex] : null;

    public Token? Peek(int offset = 1)
    {
        int i = TokenIndex + offset;
        return i < Tokens.Count ? Tokens[i] : null;
    }

    // Current token, or a parse error if we've run off the end.
    public Token Require()
    {
        return GetCurrentToken()
            ?? throw new FcParseException("Unexpected end of input");
    }
}
