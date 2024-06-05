namespace FriedChickenScript;

public class Parser
{
    public List<Token> tokens { get; private set; }
    public int TokenIndex { get; private set; } = 0;


    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public ASTNode Parse()
    {
        ASTNode root = new ASTNode(NodeType.Program);
        while (TokenIndex < tokens.Count)
        {
            root.AddChild(ParseStatement());
        }

        return root;
    }

    public ASTNode ParseStatement()
    {
        Token token = GetCurrentToken();

        switch (token.Value)
        {
            case Syntax.Variable:
                return VariableParser.ParseDeclaration(this);
            case Syntax.Object:
                return MiscParser.ParseObject(this);
            case Syntax.Function:
                return FunctionParser.ParseDeclaration(this);
            case Syntax.Return:
                return FunctionParser.ParseReturn(this);
            case Syntax.If:
                return MiscParser.ParseIfStatement(this);
            case Syntax.While:
                return MiscParser.ParseWhileStatement(this);
            default:
                // Check if the statement is an assignment
                if (GetCurrentToken()?.Type == TokenType.Identifier && tokens[TokenIndex + 1]?.Value == Syntax.Assignment)
                {
                    return VariableParser.ParseAssignment(this);
                }

                return ExpressionParser.ParseExpression(this);
        }
    }

    public Token Consume(TokenType type, string value = null)
    {
        Token token = tokens[TokenIndex];
        if (token.Type != type || (value != null && token.Value != value))
        {
            throw new Exception($"Expected {type} {value}, but got {token.Type} '{token.Value}'");
        }
        TokenIndex++;
        return token;
    }

    public Token GetCurrentToken() 
    {
        if (TokenIndex < tokens.Count)
            return tokens[TokenIndex];

        return null;
    }
}