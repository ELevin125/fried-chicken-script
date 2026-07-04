using FriedChickenScript;
using Xunit;

namespace FriedChickenScript.Tests;

public class LexerTests
{
    [Fact]
    public void StripsQuotesFromStringLiterals()
    {
        var tokens = new Lexer("\"hello chicken\"").Tokenise();
        var token = Assert.Single(tokens);
        Assert.Equal(TokenType.StringLiteral, token.Type);
        Assert.Equal("hello chicken", token.Value);
    }

    [Fact]
    public void DropsComments()
    {
        var tokens = new Lexer("// a comment\ningredient x = 5").Tokenise();
        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Comment);
        Assert.Equal(TokenType.Keyword, tokens[0].Type);
        Assert.Equal(Syntax.Variable, tokens[0].Value);
    }

    [Fact]
    public void DistinguishesKeywordsIdentifiersAndNumbers()
    {
        var tokens = new Lexer("ingredient x = 5").Tokenise();
        Assert.Collection(tokens,
            t => Assert.Equal(TokenType.Keyword, t.Type),
            t => Assert.Equal(TokenType.Identifier, t.Type),
            t => Assert.Equal(TokenType.Operator, t.Type),
            t => Assert.Equal(TokenType.Number, t.Type));
    }

    [Theory]
    [InlineData("==")]
    [InlineData("!=")]
    [InlineData(">=")]
    [InlineData("<=")]
    public void MatchesMultiCharOperatorsWhole(string op)
    {
        var tokens = new Lexer($"a {op} b").Tokenise();
        Assert.Equal(3, tokens.Count);
        Assert.Equal(TokenType.Operator, tokens[1].Type);
        Assert.Equal(op, tokens[1].Value);
    }

    [Fact]
    public void TracksLineNumbers()
    {
        var tokens = new Lexer("ingredient x = 5\ningredient y = 6").Tokenise();
        var secondIngredient = tokens.Last(t => t.Value == Syntax.Variable);
        Assert.Equal(2, secondIngredient.Line);
    }

    [Fact]
    public void UnexpectedCharacterThrows()
    {
        Assert.Throws<FcParseException>(() => new Lexer("ingredient x = $").Tokenise());
    }
}
