namespace FriedChickenScript;

public static class MiscParser
{
    public static ASTNode ParseBlock(Parser p)
    {
        ASTNode blockNode = new ASTNode(NodeType.Block);
        while (p.GetCurrentToken()?.Type != TokenType.RightBrace && p.TokenIndex < p.Tokens.Count)
        {
            blockNode.AddChild(p.ParseStatement());
        }
        return blockNode;
    }

    // print(expression)
    public static ASTNode ParsePrintStatement(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.Print);
        p.Consume(TokenType.LeftParen);
        ASTNode printNode = new ASTNode(NodeType.PrintStatement);
        printNode.AddChild(ExpressionParser.ParseExpression(p));
        p.Consume(TokenType.RightParen);
        return printNode;
    }

    public static ASTNode ParseIfStatement(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.If);
        ASTNode ifNode = new ASTNode(NodeType.IfStatement);

        p.Consume(TokenType.LeftParen);
        ifNode.AddChild(ExpressionParser.ParseExpression(p));
        p.Consume(TokenType.RightParen);

        ifNode.AddChild(ParseBracedBlock(p));

        if (p.GetCurrentToken()?.Value == Syntax.Else)
        {
            p.Consume(TokenType.Keyword, Syntax.Else);
            ifNode.AddChild(ParseBracedBlock(p));
        }

        return ifNode;
    }

    public static ASTNode ParseWhileStatement(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.While);
        ASTNode loopNode = new ASTNode(NodeType.WhileStatement);

        p.Consume(TokenType.LeftParen);
        loopNode.AddChild(ExpressionParser.ParseExpression(p));
        p.Consume(TokenType.RightParen);

        loopNode.AddChild(ParseBracedBlock(p));
        return loopNode;
    }

    private static ASTNode ParseBracedBlock(Parser p)
    {
        p.Consume(TokenType.LeftBrace);
        ASTNode block = ParseBlock(p);
        p.Consume(TokenType.RightBrace);
        return block;
    }
}
