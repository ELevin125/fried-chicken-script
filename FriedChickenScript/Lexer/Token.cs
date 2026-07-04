namespace FriedChickenScript;

public enum TokenType
{
    Keyword,
    Identifier,
    Operator,
    Number,        // integer literal: 42
    StringLiteral, // "quoted text" (quotes stripped by the lexer)
    ConstLiteral,  // COOKED / RAW / EMPTY
    Delimiter,     // ,
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    LeftBracket,   // [
    RightBracket,  // ]
    Comment
}

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(TokenType type, string value, int line = 0, int column = 0)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return $"{Type}: {Value}";
    }
}
