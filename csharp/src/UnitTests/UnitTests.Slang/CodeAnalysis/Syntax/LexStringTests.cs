using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;
    using static LexerDiagnostic.ErrorCode;

    [ExcludeFromCodeCoverage]
    public class LexStringTests
    {
        [Fact]
        public void String_is_lexed()
        {
            var source = "\"foo\"";
            var (tokens, _) = Helper.Lex(source);

            AssertEx.KindIs(new[] { DoubleQuoteToken, StringLiteralToken, DoubleQuoteToken, EofToken }, tokens);
            Assert.Equal("foo", tokens[1].Value);
        }

        [Fact]
        public void Quotes_can_be_escaped()
        {
            var source = "\"foo\\\"\""; // This is "foo\""
            var (tokens, _) = Helper.Lex(source);

            // NB: the lexer does not try to interpret the contents of the string
            // Therefore, what we get is 'foo\"' and not 'foo"'

            AssertEx.KindIs(new[] { DoubleQuoteToken, StringLiteralToken, DoubleQuoteToken, EofToken }, tokens);
            Assert.Equal("foo\\\"", tokens[1].Value);
        }

        [Fact]
        public void Missing_end_quotes_generates_a_diagnostic()
        {
            var source = "\"foo";
            var (tokens, diags) = Helper.Lex(source);

            AssertEx.KindIs(new[] { DoubleQuoteToken, StringLiteralToken, EofToken }, tokens);
            Assert.Equal("foo", tokens[1].Value);
            Assert.Equal(UnterminatedString.ToId(), diags[0].Id);
        }
    }
}
