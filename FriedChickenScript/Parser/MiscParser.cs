namespace FriedChickenScript;

public static class MiscParser
{
    public static ASTNode ParseBlock(Parser p)
    {
        ASTNode blockNode = new ASTNode(NodeType.Block);

        while (p.GetCurrentToken().Type != TokenType.RightBrace && p.TokenIndex < p.tokens.Count)
        {
            blockNode.AddChild(p.ParseStatement());
        }

        return blockNode;
    }

    public static ASTNode ParseIfStatement(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.If);

        ASTNode ifNode = new ASTNode(NodeType.IfStatement);

        // Parse condition
        p.Consume(TokenType.LeftParen);
        ifNode.AddChild(ExpressionParser.ParseExpression(p));
        p.Consume(TokenType.RightParen);

        // Parse if block
        p.Consume(TokenType.LeftBrace);
        ifNode.AddChild(MiscParser.ParseBlock(p));
        p.Consume(TokenType.RightBrace);

        // Check for else block
        if (p.GetCurrentToken()?.Value == Syntax.Else)
        {
            p.Consume(TokenType.Keyword, Syntax.Else);

            // Parse else block
            p.Consume(TokenType.LeftBrace);
            ifNode.AddChild(MiscParser.ParseBlock(p));
            p.Consume(TokenType.RightBrace);
        }

        return ifNode;
    }

    public static ASTNode ParseWhileStatement(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.While);

        ASTNode loopNode = new ASTNode(NodeType.WhileStatement);

        // Parse condition
        p.Consume(TokenType.LeftParen);
        loopNode.AddChild(ExpressionParser.ParseExpression(p));
        p.Consume(TokenType.RightParen);

        // Parse if block
        p.Consume(TokenType.LeftBrace);
        loopNode.AddChild(MiscParser.ParseBlock(p));
        p.Consume(TokenType.RightBrace);

        return loopNode;
    }
}
