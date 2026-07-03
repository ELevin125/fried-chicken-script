namespace FriedChickenScript;

// Expression grammar via precedence climbing:
//
//   expression → binary(0)
//   binary(min)→ postfix { operator[prec>=min] binary(prec+1) }   (left-associative)
//   postfix    → atom { '(' args ')' | '.' identifier }
//   atom       → literal | identifier | '(' expression ')'
//
// A single precedence table drives associativity and binding strength, which is what makes
// `2 + 3 * 4` == 14 and `10 - 2 - 3` == 5.
public static class ExpressionParser
{
    // Higher number = binds tighter.
    private static readonly Dictionary<string, int> BinaryPrecedence = new()
    {
        [Syntax.Or]            = 1,
        [Syntax.And]           = 2,
        [Syntax.Equality]      = 3,
        [Syntax.Inequality]    = 3,
        [Syntax.LessThan]      = 4,
        [Syntax.GreaterThan]   = 4,
        [Syntax.EqLessThan]    = 4,
        [Syntax.EqGreaterThan] = 4,
        [Syntax.Addition]      = 5,
        [Syntax.Subtraction]   = 5,
        [Syntax.Multiplication]= 6,
        [Syntax.Division]      = 6,
    };

    public static ASTNode ParseExpression(Parser p) => ParseBinary(p, 0);

    private static ASTNode ParseBinary(Parser p, int minPrecedence)
    {
        ASTNode left = ParsePostfix(p);

        while (true)
        {
            Token? op = p.GetCurrentToken();
            if (op == null || op.Type != TokenType.Operator
                || !BinaryPrecedence.TryGetValue(op.Value, out int precedence)
                || precedence < minPrecedence)
                break;

            p.Consume(TokenType.Operator, op.Value);
            // +1 => left-associative (same-precedence operators group left to right).
            ASTNode right = ParseBinary(p, precedence + 1);

            ASTNode binary = new ASTNode(NodeType.BinaryExpression, op.Value);
            binary.AddChild(left);
            binary.AddChild(right);
            left = binary;
        }

        return left;
    }

    // Function calls and member access bind tighter than any binary operator.
    private static ASTNode ParsePostfix(Parser p)
    {
        ASTNode node = ParseAtom(p);

        while (true)
        {
            Token? next = p.GetCurrentToken();
            if (next == null)
                break;

            if (next.Type == TokenType.LeftParen && node.Type == NodeType.Identifier)
            {
                node = FunctionParser.ParseFunctionCall(p, node.Value!);
            }
            else if (next.Type == TokenType.Operator && next.Value == Syntax.Dot)
            {
                p.Consume(TokenType.Operator, Syntax.Dot);
                Token field = p.Consume(TokenType.Identifier);
                ASTNode access = new ASTNode(NodeType.MemberAccess, field.Value);
                access.AddChild(node);
                node = access;
            }
            else
            {
                break;
            }
        }

        return node;
    }

    private static ASTNode ParseAtom(Parser p)
    {
        Token token = p.Require();

        switch (token.Type)
        {
            case TokenType.Number:
                p.Consume(TokenType.Number);
                return new ASTNode(NodeType.NumberLiteral, token.Value);

            case TokenType.StringLiteral:
                p.Consume(TokenType.StringLiteral);
                return new ASTNode(NodeType.StringLiteral, token.Value);

            case TokenType.ConstLiteral:
                p.Consume(TokenType.ConstLiteral);
                return ConstLiteral(token.Value);

            case TokenType.Identifier:
                p.Consume(TokenType.Identifier);
                return new ASTNode(NodeType.Identifier, token.Value);

            case TokenType.LeftParen:
                p.Consume(TokenType.LeftParen);
                ASTNode inner = ParseExpression(p);
                p.Consume(TokenType.RightParen);
                return inner;

            default:
                throw new FcParseException($"Unexpected token {token.Type} '{token.Value}' at line {token.Line}");
        }
    }

    private static ASTNode ConstLiteral(string value)
    {
        if (value == Syntax.True)
            return new ASTNode(NodeType.BooleanLiteral, "true");
        if (value == Syntax.False)
            return new ASTNode(NodeType.BooleanLiteral, "false");
        return new ASTNode(NodeType.NullLiteral);
    }
}
