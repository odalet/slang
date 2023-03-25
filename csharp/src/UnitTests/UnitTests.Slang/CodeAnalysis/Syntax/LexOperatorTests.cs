using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;

    [ExcludeFromCodeCoverage]
    public class LexOperatorTests
    {
        [Theory]
        [InlineData("+", new[] { PlusToken })]
        [InlineData("-", new[] { MinusToken })]
        [InlineData("*", new[] { StarToken })]
        [InlineData("/", new[] { SlashToken })]
        [InlineData("(", new[] { LeftParenToken })]
        [InlineData(")", new[] { RightParenToken })]
        [InlineData("{", new[] { LeftBraceToken })]
        [InlineData("}", new[] { RightBraceToken })]
        [InlineData(".", new[] { DotToken })]
        [InlineData(",", new[] { CommaToken })]
        [InlineData(":", new[] { ColonToken })]
        [InlineData(";", new[] { SemicolonToken })]
        [InlineData("<", new[] { LessToken })]
        [InlineData(">", new[] { GreaterToken })]
        [InlineData("=", new[] { EqualToken })]
        [InlineData("!", new[] { BangToken })]
        [InlineData("<=", new[] { LessEqualToken })]
        [InlineData(">=", new[] { GreaterEqualToken })]
        [InlineData("==", new[] { EqualEqualToken })]
        [InlineData("!=", new[] { BangEqualToken })]
        // Below, are strings that resolve to 2 operators
        [InlineData("+=", new[] { PlusToken, EqualToken })]
        [InlineData("-=", new[] { MinusToken, EqualToken })]
        [InlineData("*=", new[] { StarToken, EqualToken })]
        [InlineData("/=", new[] { SlashToken, EqualToken })]
        public void Operators_are_correctly_recognized(string source, SyntaxKind[] expectedTokens)
        {
            var (tokens, _) = Helper.Lex(source);

            var expected = expectedTokens.Append(EofToken);
            AssertEx.KindIs(expected, tokens);
        }
    }
}
