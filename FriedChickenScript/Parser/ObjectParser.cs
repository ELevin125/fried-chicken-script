namespace FriedChickenScript;

public static class ObjectParser
{
    // Type definition:
    //   bucket Order {
    //       ingredient recipeName
    //       ingredient pieces = 8
    //   }
    // Fields are declared with `ingredient name`, optionally with a default value.
    public static ASTNode ParseTypeDefinition(Parser p)
    {
        p.Consume(TokenType.Keyword, Syntax.Object);
        Token name = p.Consume(TokenType.Identifier);
        ASTNode typeNode = new ASTNode(NodeType.BucketDeclaration, name.Value);

        p.Consume(TokenType.LeftBrace);
        while (p.GetCurrentToken()?.Type != TokenType.RightBrace && p.TokenIndex < p.Tokens.Count)
        {
            p.Consume(TokenType.Keyword, Syntax.Variable);
            Token fieldName = p.Consume(TokenType.Identifier);
            ASTNode field = new ASTNode(NodeType.FieldDeclaration, fieldName.Value);

            if (p.GetCurrentToken()?.Type == TokenType.Operator && p.GetCurrentToken()?.Value == Syntax.Assignment)
            {
                p.Consume(TokenType.Operator, Syntax.Assignment);
                field.AddChild(ExpressionParser.ParseExpression(p));
            }

            typeNode.AddChild(field);
        }
        p.Consume(TokenType.RightBrace);
        return typeNode;
    }

    // Instantiation:
    //   Order myOrder
    //   Meal  myMeal = cookChicken(...)
    public static ASTNode ParseInstantiation(Parser p)
    {
        Token typeName = p.Consume(TokenType.Identifier);
        Token varName = p.Consume(TokenType.Identifier);

        ASTNode node = new ASTNode(NodeType.ObjectInstantiation, typeName.Value);
        node.AddChild(new ASTNode(NodeType.Identifier, varName.Value));

        if (p.GetCurrentToken()?.Type == TokenType.Operator && p.GetCurrentToken()?.Value == Syntax.Assignment)
        {
            p.Consume(TokenType.Operator, Syntax.Assignment);
            node.AddChild(ExpressionParser.ParseExpression(p));
        }

        return node;
    }
}
