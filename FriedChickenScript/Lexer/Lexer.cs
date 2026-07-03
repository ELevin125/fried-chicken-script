using System.Text.RegularExpressions;

namespace FriedChickenScript;

public class Lexer
{
    private readonly string src;
    private int pos = 0;
    private int line = 1;
    private int col = 1;

    public Lexer(string src)
    {
        this.src = src;
    }

    public List<Token> Tokenise()
    {
        var tokens = new List<Token>();

        while (pos < src.Length)
        {
            char c = src[pos];
            if (c == '\n')
            {
                pos++;
                line++;
                col = 1;
                continue;
            }
            if (char.IsWhiteSpace(c))
            {
                pos++;
                col++;
                continue;
            }

            if (!TryMatch(tokens))
                throw new FcParseException($"Unexpected character '{c}' at line {line}, column {col}");
        }

        return tokens;
    }

    // Try each rule in order; the first that matches at the current position wins.
    private bool TryMatch(List<Token> tokens)
    {
        foreach (var (regex, type) in TokenPatterns.Patterns)
        {
            Match m = regex.Match(src, pos);
            if (!m.Success)
                continue;

            if (type != TokenType.Comment)
            {
                // String literals keep only their inner text (capture group 1).
                string value = type == TokenType.StringLiteral ? m.Groups[1].Value : m.Value;
                tokens.Add(new Token(type, value, line, col));
            }

            Advance(m.Value);
            return true;
        }
        return false;
    }

    // Move past matched text, keeping line/column accurate even across newlines
    // (block comments and strings can span lines).
    private void Advance(string text)
    {
        foreach (char ch in text)
        {
            if (ch == '\n')
            {
                line++;
                col = 1;
            }
            else
            {
                col++;
            }
        }
        pos += text.Length;
    }
}
