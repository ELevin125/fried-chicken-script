namespace FriedChickenScript;

public static class FunctionParser
{
    public static ASTNode ParseReturn(Parser p)
    {
        ASTNode returnNode = new ASTNode(NodeType.ReturnStatement);
        p.Consume(TokenType.Keyword, Syntax.Return);
        returnNode.AddChild(ExpressionParser.ParseExpression(p));
        return returnNode;
    }

    public static ASTNode ParseDeclaration(Parser p)
    {
        // func identifier { }
        // func identifier withParams: paramA, paramB { }

        p.Consume(TokenType.Keyword, Syntax.Function);

        Token name = p.Consume(TokenType.Identifier);

        ASTNode funcNode = new ASTNode(NodeType.FunctionDeclaration, name.Value);

        if (p.GetCurrentToken().Value == Syntax.Parameter)
        {
            p.Consume(TokenType.Keyword, Syntax.Parameter);
            while (p.GetCurrentToken().Type != TokenType.LeftBrace)
            {
                // 
                Token param = p.Consume(TokenType.Identifier);
                ASTNode paramNode = new ASTNode(NodeType.Parameter, param.Value);
                if (p.GetCurrentToken().Value == Syntax.Assignment)
                {
                    p.Consume(TokenType.Operator, Syntax.Assignment);
                    paramNode.AddChild(new ASTNode(NodeType.Literal, p.GetCurrentToken().Value));
                    p.Consume(TokenType.Literal);
                }
                else
                {
                    paramNode.AddChild(new ASTNode(NodeType.Literal, null));
                }

                funcNode.AddChild(paramNode);

                if (p.GetCurrentToken().Type == TokenType.Delimiter)
                {
                    p.Consume(TokenType.Delimiter);
                }
            }
        }

        p.Consume(TokenType.LeftBrace);
        funcNode.AddChild(MiscParser.ParseBlock(p));
        p.Consume(TokenType.RightBrace);
        return funcNode;
    }

    public static ASTNode ParseFunctionCall(Parser p, Token functionNameToken)
    {
        ASTNode funcCallNode = new ASTNode(NodeType.FunctionCall, functionNameToken.Value);
        p.Consume(TokenType.LeftParen);

        ASTNode args = new ASTNode(NodeType.Arguments);
        while (p.GetCurrentToken().Type != TokenType.RightParen)
        {
            ASTNode argument = ExpressionParser.ParseExpression(p);
            args.AddChild(argument);

            if (p.GetCurrentToken().Type == TokenType.Delimiter)
                p.Consume(TokenType.Delimiter);
            else
                break;
        }
        if (args.Children.Count > 0)
        {
            funcCallNode.AddChild(args);
        }
        p.Consume(TokenType.RightParen);
        return funcCallNode;
    }
}
