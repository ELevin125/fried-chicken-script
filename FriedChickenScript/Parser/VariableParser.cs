namespace FriedChickenScript;

public static class VariableParser
{
    public static ASTNode ParseDeclaration(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.Variable);
        Token name = p.Consume(TokenType.Identifier);
        p.Consume(TokenType.Operator, Syntax.Assignment);

        ASTNode varNode = new ASTNode(NodeType.VariableDeclaration, name.Value);
        varNode.AddChild(ExpressionParser.ParseExpression(p));
        return varNode;
    }

    public static ASTNode ParseAssignment(Parser p)
    {
        Token identifierToken = p.Consume(TokenType.Identifier);
        p.Consume(TokenType.Operator, Syntax.Assignment);

        ASTNode assignmentNode = new ASTNode(NodeType.Assignment, identifierToken.Value);

        assignmentNode.AddChild(ExpressionParser.ParseExpression(p));

        return assignmentNode;
    }
}
