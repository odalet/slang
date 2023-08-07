using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Slang.CodeAnalysis.Syntax;

using static SyntaxKind;

[ExcludeFromCodeCoverage]
public class LexIdentifierTests
{
    [Theory]
    [InlineData("foo")]
    [InlineData("_foo")]
    [InlineData("foo123")]
    [InlineData("_foo123")]
    [InlineData("é")]
    [InlineData("ç")]
    public void Non_reserved_words_are_identifiers(string source)
    {
        var (tokens, _) = Helper.Lex(source);
        AssertEx.KindIs(new[] { IdentifierToken, EofToken }, tokens);
    }

    [Theory, MemberData(nameof(GetReservedWords))]
    public void Reserved_words_are_keywords(string source)
    {
        var (tokens, _) = Helper.Lex(source);
        Assert.True(Lexer.IsReservedWordToken(tokens[0].Kind));
    }

    public static IEnumerable<object[]> GetReservedWords()
    {
        foreach (var reservedWord in Lexer.ReservedWords)
            yield return new[] { reservedWord };
    }
}
