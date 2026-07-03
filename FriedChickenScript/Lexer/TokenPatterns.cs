using System.Text.RegularExpressions;

namespace FriedChickenScript;

// The ordered list of token rules. Order is significant and guaranteed (a List, not a
// Dictionary): comments before '/', keywords before identifiers, and multi-character
// operators before their single-character prefixes (== before =, <= before <, ...).
//
// Each regex is anchored with \G and compiled once, so the lexer matches only at its
// current position without allocating substrings.
public static class TokenPatterns
{
    public static readonly IReadOnlyList<(Regex Regex, TokenType Type)> Patterns = Build();

    private static List<(Regex, TokenType)> Build()
    {
        (string Pattern, TokenType Type)[] rules =
        {
            (@"//[^\n]*",                          TokenType.Comment),
            (@"/\*[\s\S]*?\*/",                    TokenType.Comment),

            ($@"\b{Syntax.Variable}\b",            TokenType.Keyword),
            ($@"\b{Syntax.Object}\b",              TokenType.Keyword),
            ($@"\b{Syntax.Function}\b",            TokenType.Keyword),
            (Regex.Escape(Syntax.Parameter),       TokenType.Keyword),
            ($@"\b{Syntax.Return}\b",              TokenType.Keyword),
            ($@"\b{Syntax.If}\b",                  TokenType.Keyword),
            ($@"\b{Syntax.Else}\b",                TokenType.Keyword),
            ($@"\b{Syntax.While}\b",               TokenType.Keyword),
            ($@"\b{Syntax.Print}\b",               TokenType.Keyword),

            ($@"\b{Syntax.True}\b",                TokenType.ConstLiteral),
            ($@"\b{Syntax.False}\b",               TokenType.ConstLiteral),
            ($@"\b{Syntax.Null}\b",                TokenType.ConstLiteral),

            (@"[a-zA-Z_][a-zA-Z0-9_]*",            TokenType.Identifier),
            (@",",                                 TokenType.Delimiter),

            (Regex.Escape(Syntax.And),             TokenType.Operator),
            (Regex.Escape(Syntax.Or),              TokenType.Operator),
            (Regex.Escape(Syntax.Equality),        TokenType.Operator),
            (Regex.Escape(Syntax.Inequality),      TokenType.Operator),
            (Regex.Escape(Syntax.EqGreaterThan),   TokenType.Operator),
            (Regex.Escape(Syntax.EqLessThan),      TokenType.Operator),
            (Regex.Escape(Syntax.LessThan),        TokenType.Operator),
            (Regex.Escape(Syntax.GreaterThan),     TokenType.Operator),
            (Regex.Escape(Syntax.Assignment),      TokenType.Operator),
            (Regex.Escape(Syntax.Addition),        TokenType.Operator),
            (Regex.Escape(Syntax.Subtraction),     TokenType.Operator),
            (Regex.Escape(Syntax.Division),        TokenType.Operator),
            (Regex.Escape(Syntax.Multiplication),  TokenType.Operator),
            (Regex.Escape(Syntax.Dot),             TokenType.Operator),

            (@"[0-9]+",                            TokenType.Number),
            ("\"([^\"]*)\"",                       TokenType.StringLiteral),

            (@"\(",                                TokenType.LeftParen),
            (@"\)",                                TokenType.RightParen),
            (@"\{",                                TokenType.LeftBrace),
            (@"\}",                                TokenType.RightBrace),
        };

        var patterns = new List<(Regex, TokenType)>(rules.Length);
        foreach (var (pattern, type) in rules)
        {
            patterns.Add((new Regex(@"\G(?:" + pattern + ")", RegexOptions.Compiled), type));
        }
        return patterns;
    }
}
