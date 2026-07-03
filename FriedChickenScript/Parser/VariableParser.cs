namespace FriedChickenScript;

public static class VariableParser
{
    // ingredient name = expression
    public static ASTNode ParseDeclaration(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.Variable);
        Token name = p.Consume(TokenType.Identifier);
        p.Consume(TokenType.Operator, Syntax.Assignment);

        ASTNode varNode = new ASTNode(NodeType.VariableDeclaration, name.Value);
        varNode.AddChild(ExpressionParser.ParseExpression(p));
        return varNode;
    }
}
