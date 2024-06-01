namespace FriedChickenScript;

public class Parser
{
    private List<Token> tokens;
    private int tokenIndex = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public ASTNode Parse()
    {
        ASTNode root = new ASTNode(NodeType.Program);
        while (tokenIndex < tokens.Count)
        {
            root.AddChild(ParseStatement());
        }

        return root;
    }

    private ASTNode ParseStatement()
    {
        Token token = tokens[tokenIndex];

        switch (token.Value)
        {
            case Syntax.Variable:
                return ParseVariableDeclaration();
        }

        return null;
    }


    private ASTNode ParseVariableDeclaration()
    {
        Consume(TokenType.Keyword, Syntax.Variable);

        Token name = Consume(TokenType.Identifier);
        Consume(TokenType.Operator, Syntax.Assignment);

        ASTNode varNode = new ASTNode(NodeType.VariableDeclaration, name.Value);
        varNode.AddChild(ParseExpression());

        return varNode;
    }
    private ASTNode ParseExpression()
    {
        // For simplicity, assume the expression is just a literal value
        Token valueToken = Consume(TokenType.Literal);

        return new ASTNode(NodeType.Literal, valueToken.Value);
    }

    private Token Consume(TokenType type, string value = null)
    {
        Token token = tokens[tokenIndex];
        if (token.Type != type || (value != null && token.Value != value))
        {
            throw new Exception($"Expected {type} {value}, but got {token.Type} {token.Value}");
        }
        tokenIndex++;
        return token;
    }
}