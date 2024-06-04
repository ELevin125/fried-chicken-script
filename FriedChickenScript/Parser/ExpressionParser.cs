namespace FriedChickenScript;

public static class ExpressionParser
{
    public static ASTNode ParseExpression(Parser p)
    {
        ASTNode parsed = ParsePrimary(p);
        parsed = ParseLogicalAndOr(p, parsed);
        parsed = ParseMultDiv(p, parsed);
        parsed = ParseAddSub(p, parsed);
        parsed = ParseComparison(p, parsed);
        //parsed = ParseLogicalAndOr(p, parsed);
        return parsed;
    }

    private static ASTNode ParsePrimary(Parser p)
    {
        Token currentToken = p.GetCurrentToken();

        if (currentToken.Type == TokenType.Literal)
        {
            p.Consume(TokenType.Literal);
            return new ASTNode(NodeType.Literal, currentToken.Value);
        }
        else if (currentToken.Type == TokenType.ConstLiteral)
        {
            p.Consume(TokenType.ConstLiteral);
            string value = ExpressionParser.GetConstLiteralVal(currentToken.Value);
            return new ASTNode(NodeType.Literal, value);
        }
        else if (currentToken.Type == TokenType.Identifier)
        {
            Token identifierToken = p.Consume(TokenType.Identifier);
            if (p.GetCurrentToken()?.Type == TokenType.LeftParen)
            {
                return FunctionParser.ParseFunctionCall(p, identifierToken);
            }
            return new ASTNode(NodeType.Identifier, identifierToken.Value);
        }
        else if (currentToken.Type == TokenType.LeftParen)
        {
            p.Consume(TokenType.LeftParen, "(");
            ASTNode expression = ExpressionParser.ParseExpression(p);
            p.Consume(TokenType.RightParen, ")");
            return expression;
        }
        else
        {
            throw new Exception("Unexpected token: " + currentToken.Type);
        }
    }

    private static string GetConstLiteralVal(string value)
    {
        switch (value)
        {
            case Syntax.True: 
                return "true";
            
            case Syntax.False: 
                return "false";
            
            default: 
                return null;
        }
    }

    private static ASTNode ParseAddSub(Parser p, ASTNode current)
    {
        ASTNode node = current;

        while (p.GetCurrentToken()?.Type == TokenType.Operator
           && (p.GetCurrentToken()?.Value == Syntax.Addition || p.GetCurrentToken()?.Value == Syntax.Subtraction))
        {
            Token operatorToken = p.Consume(TokenType.Operator, p.GetCurrentToken().Value);
            ASTNode right = ExpressionParser.ParseExpression(p);
            ASTNode binaryExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            binaryExpression.AddChild(node);
            binaryExpression.AddChild(right);
            node = binaryExpression;
        }

        return node;
    }

    private static ASTNode ParseMultDiv(Parser p, ASTNode current)
    {
        ASTNode node = current;

        while (p.GetCurrentToken()?.Type == TokenType.Operator
           && (p.GetCurrentToken()?.Value == Syntax.Multiplication) || p.GetCurrentToken()?.Value == Syntax.Division)
        {
            Token operatorToken = p.Consume(TokenType.Operator, p.GetCurrentToken().Value);
            ASTNode right = ExpressionParser.ParsePrimary(p);
            ASTNode binaryExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            binaryExpression.AddChild(node);
            binaryExpression.AddChild(right);
            node = binaryExpression;
        }

        return node;
    }

    private static ASTNode ParseComparison(Parser p, ASTNode current)
    {
        ASTNode node = current;

        while (p.GetCurrentToken()?.Type == TokenType.Operator
           && (p.GetCurrentToken()?.Value == Syntax.LessThan || p.GetCurrentToken()?.Value == Syntax.GreaterThan ||
               p.GetCurrentToken()?.Value == "<=" || p.GetCurrentToken()?.Value == ">=" ||
               p.GetCurrentToken()?.Value == "==" || p.GetCurrentToken()?.Value == "!="))
        {
            Token operatorToken = p.Consume(TokenType.Operator);
            ASTNode right = ExpressionParser.ParsePrimary(p);
            ASTNode comparisonExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            comparisonExpression.AddChild(node);
            comparisonExpression.AddChild(right);
            node = comparisonExpression;
        }

        return node;
    }

    private static ASTNode ParseLogicalAndOr(Parser p, ASTNode current)
    {
        ASTNode node = current;

        while (p.GetCurrentToken()?.Type == TokenType.Operator
           && (p.GetCurrentToken()?.Value == Syntax.And) || p.GetCurrentToken()?.Value == Syntax.Or)
        {
            Token operatorToken = p.Consume(TokenType.Operator, p.GetCurrentToken().Value);
            ASTNode right = ExpressionParser.ParsePrimary(p);
            ASTNode binaryExpression = new ASTNode(NodeType.BinaryExpression, operatorToken.Value);
            binaryExpression.AddChild(node);
            binaryExpression.AddChild(right);
            node = binaryExpression;
        }

        return node;
    }
}
