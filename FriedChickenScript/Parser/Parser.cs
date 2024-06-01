using System.Diagnostics;

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
            //default:
            //    return ParseExpression();
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

        return ParseAddition();
    }

    private ASTNode ParseAddition()
    {
        ASTNode left = ParseMultiplication(); // Parse the left operand

        while (GetCurrentToken()?.Type == TokenType.Operator
           && (GetCurrentToken()?.Value == Syntax.Addition || GetCurrentToken()?.Value == Syntax.Subtraction))
        {
            Token operatorToken = Consume(TokenType.Operator, GetCurrentToken().Value);
            ASTNode right = ParseMultiplication();
            ASTNode binaryExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            binaryExpression.AddChild(left);
            binaryExpression.AddChild(right);
            left = binaryExpression; // Update left to be the binary expression node
        }

        return left;
    }


    private ASTNode ParseMultiplication()
    {
        ASTNode left = ParsePrimary(); // Parse the left operand

        while (GetCurrentToken()?.Type == TokenType.Operator
           && (GetCurrentToken()?.Value == Syntax.Multiplication) || GetCurrentToken()?.Value == Syntax.Division)
        {
            Token operatorToken = Consume(TokenType.Operator, GetCurrentToken().Value);
            ASTNode right = ParsePrimary();
            ASTNode binaryExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            binaryExpression.AddChild(left);
            binaryExpression.AddChild(right);
            left = binaryExpression; // Update left to be the binary expression node
        }

        return left;
    }

    private ASTNode ParsePrimary()
    {
        Token currentToken = GetCurrentToken();

        if (currentToken.Type == TokenType.Literal)
        {
            Consume(TokenType.Literal);
            return new ASTNode(NodeType.Literal, currentToken.Value);
        }
        else if (currentToken.Type == TokenType.Identifier)
        {
            Consume(TokenType.Literal);
            return new ASTNode(NodeType.Identifier, currentToken.Value);
        }
        else if (currentToken.Type == TokenType.LeftParen)
        {
            Consume(TokenType.LeftParen, "(");
            ASTNode expression = ParseExpression();
            Consume(TokenType.RightParen, ")");
            return expression;
        }
        else
        {
            throw new Exception("Unexpected token: " + currentToken.Type);
        }
    }


    //private ASTNode ParseExpression()
    //{
    //    // Handle binary expressions and function calls
    //    ASTNode left = ParsePrimary();

    //    if (tokens[tokenIndex].Type == TokenType.Operator && tokens[tokenIndex].Value == "+")
    //    {
    //        Token op = Consume(TokenType.Operator, "+");
    //        ASTNode right = ParsePrimary();
    //        ASTNode binaryExpr = new ASTNode(NodeType.BinaryExpression, op.Value);
    //        binaryExpr.AddChild(left);
    //        binaryExpr.AddChild(right);
    //        return binaryExpr;
    //    }

    //    return left;
    //}

    //private ASTNode ParsePrimary()
    //{
    //    Token token = tokens[tokenIndex];

    //    if (token.Type == TokenType.Identifier)
    //    {
    //        tokenIndex++;
    //        if (tokens[tokenIndex].Type == TokenType.LeftParen)
    //        {
    //            // Function call
    //            Consume(TokenType.LeftParen);
    //            ASTNode funcCall = new ASTNode(NodeType.FunctionCall, token.Value);
    //            funcCall.AddChild(ParseExpression());
    //            Consume(TokenType.RightParen);
    //            return funcCall;
    //        }
    //        return new ASTNode(NodeType.Identifier, token.Value);
    //    }

    //    if (token.Type == TokenType.Number)
    //    {
    //        tokenIndex++;
    //        return new ASTNode(NodeType.Number, token.Value);
    //    }

    //    throw new Exception($"Unexpected token: {token}");
    //}

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

    private Token GetCurrentToken() 
    {
        if (tokenIndex < tokens.Count)
            return tokens[tokenIndex];

        return null;
    }
}