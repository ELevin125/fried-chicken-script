namespace FriedChickenScript;

public enum TokenType
{
    Keyword,
    Identifier,
    Operator,
    Literal,
    Paren,
    Brace,
    Comment
}

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Type}: {Value}";
    }
}
