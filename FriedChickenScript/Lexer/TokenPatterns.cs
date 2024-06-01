namespace FriedChickenScript;

public static class TokenPatterns
{
    public static readonly Dictionary<string, TokenType> Patterns = new Dictionary<string, TokenType> {
        { @"\/\/.*", TokenType.Comment },
        { $@"\b{Syntax.Variable}\b", TokenType.Keyword },
        { $@"\b{Syntax.Function}\b", TokenType.Keyword },
        { $@"\b{Syntax.Return}\b", TokenType.Keyword },
        { $@"\b{Syntax.Print}\b", TokenType.Keyword },
        { @"[a-zA-Z_][a-zA-Z0-9_]*", TokenType.Identifier }, // variable / func name
        { @"=", TokenType.Operator },
        { @"\+", TokenType.Operator },
        { @"\-", TokenType.Operator },
        { @"\/", TokenType.Operator },
        { @"\*", TokenType.Operator },
        { @"[0-9]+", TokenType.Literal }, // number
        { @"""[^""]*""", TokenType.Literal }, // string ""
        { @"\(", TokenType.LeftParen },
        { @"\)", TokenType.RightParen },
        { @"\{", TokenType.LeftBrace },
        { @"\}", TokenType.RightBrace },
    };
}