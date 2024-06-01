namespace FriedChickenScript;

public static class TokenPatterns
{
    public static readonly Dictionary<string, TokenType> Patterns = new Dictionary<string, TokenType> {
        { @"\/\/.*", TokenType.Comment },
        { @"\bingredient\b", TokenType.Keyword }, // variable
        { @"\brecipe\b", TokenType.Keyword }, // function
        { @"\bserve\b", TokenType.Keyword }, // return
        { @"\bprint\b", TokenType.Keyword },
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