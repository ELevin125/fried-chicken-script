using System.Diagnostics;
using System.Text.RegularExpressions;
namespace FriedChickenScript;

public class Lexer
{
    private string rawProgram;
    private int matchPos = 0;

    public Lexer(string rawProgram)
    {
        this.rawProgram = rawProgram;
    }

    public List<Token> Tokenise()
    {
        List<Token> tokens = new List<Token>();
        while (matchPos < rawProgram.Length)
        {
            if (char.IsWhiteSpace(rawProgram[matchPos]))
            {
                matchPos++;
                continue;
            }

            Token token = NextToken();

            if (token != null)
            {
                if (token.Type != TokenType.Comment)
                    tokens.Add(token);
            }
            else
            {
                throw new Exception($"Unexpected character: {rawProgram[matchPos]}");
            }
        }

        return tokens;
    }

    private Token NextToken()
    {
        foreach (var pattern in TokenPatterns.Patterns)
        {
            Regex regex = new Regex($"^{pattern.Key}");
            Match match = regex.Match(rawProgram.Substring(matchPos));

            if (match.Success)
            {
                matchPos += match.Length;
                return new Token(pattern.Value, match.Value);
            }
        }
        return null;
    }
}
