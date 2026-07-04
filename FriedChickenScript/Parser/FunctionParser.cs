namespace FriedChickenScript;

public static class FunctionParser
{
    public static ASTNode ParseReturn(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.Return);
        ASTNode returnNode = new ASTNode(NodeType.ReturnStatement);
        returnNode.AddChild(ExpressionParser.ParseExpression(p));
        return returnNode;
    }

    // recipe name withExtra: p1, p2 { ... }
    // recipe name(p1, p2)        { ... }
    // recipe name                { ... }
    public static ASTNode ParseDeclaration(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.Function);
        Token name = p.Consume(TokenType.Identifier);
        ASTNode funcNode = new ASTNode(NodeType.FunctionDeclaration, name.Value);

        if (p.GetCurrentToken()?.Value == Syntax.Parameter)
        {
            p.Consume(TokenType.Keyword, Syntax.Parameter);
            while (p.GetCurrentToken()?.Type != TokenType.LeftBrace)
            {
                ParseParameter(p, funcNode);
            }
        }
        else if (p.GetCurrentToken()?.Type == TokenType.LeftParen)
        {
            p.Consume(TokenType.LeftParen);
            while (p.GetCurrentToken()?.Type != TokenType.RightParen)
            {
                ParseParameter(p, funcNode);
            }
            p.Consume(TokenType.RightParen);
        }

        p.Consume(TokenType.LeftBrace);
        funcNode.AddChild(MiscParser.ParseBlock(p));
        p.Consume(TokenType.RightBrace);
        return funcNode;
    }

    private static void ParseParameter(Parser p, ASTNode funcNode)
    {
        Token param = p.Consume(TokenType.Identifier);
        funcNode.AddChild(new ASTNode(NodeType.Parameter, param.Value));
        if (p.GetCurrentToken()?.Type == TokenType.Delimiter)
            p.Consume(TokenType.Delimiter);
    }

    public static ASTNode ParseFunctionCall(Parser p, string functionName)
    {
        ASTNode callNode = new ASTNode(NodeType.FunctionCall, functionName);
        ASTNode? args = ParseArguments(p);
        if (args != null)
        {
            callNode.AddChild(args);
        }
        return callNode;
    }

    // Parse a parenthesised argument list `( a, b, ... )`. Returns an Arguments node, or
    // null when there are no arguments. Shared by function calls and method calls.
    public static ASTNode? ParseArguments(Parser p)
    {
        p.Consume(TokenType.LeftParen);

        ASTNode args = new ASTNode(NodeType.Arguments);
        while (p.GetCurrentToken()?.Type != TokenType.RightParen)
        {
            args.AddChild(ExpressionParser.ParseExpression(p));
            if (p.GetCurrentToken()?.Type == TokenType.Delimiter)
            {
                p.Consume(TokenType.Delimiter);
            }
            else
            {
                break;
            }
        }

        p.Consume(TokenType.RightParen);
        return args.Children.Count > 0 ? args : null;
    }
}
