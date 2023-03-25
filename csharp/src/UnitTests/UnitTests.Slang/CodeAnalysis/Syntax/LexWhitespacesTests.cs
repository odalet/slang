using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;
    using static LexerDiagnostic.ErrorCode;

    [ExcludeFromCodeCoverage]
    public class LexWhitespacesTests
    {
        [Fact]
        public void Whitespaces_are_aggregated()
        {
            var source = "   \r\n   \n   \t   ";
            var (tokens, _) = Helper.Lex(source);

            AssertEx.KindIs(new[] { WhitespaceToken, EofToken }, tokens);
        }

        [Fact]
        public void Weird_whitespaces_are_invalid()
        {
            var source = "\xA0"; // this is &nbsp
            var (tokens, diags) = Helper.Lex(source);

            AssertEx.KindIs(new[] { InvalidToken, EofToken }, tokens);
            Assert.Equal(InvalidCharacter.ToId(), diags[0].Id);
        }

        [Fact]
        public void Lines_are_incremented()
        {
            // Line increment occurs on \r\n, \n or \r

            var source = "A\r\nB\nC\rD\n\nE\r\rF\r\r\n";
            var (tokens, _) = Helper.Lex(source);

            var expectedTokenKinds = new List<SyntaxKind>();

            for (var i = 0; i < 6; i++) // 6: A -> F
            {
                expectedTokenKinds.Add(IdentifierToken);
                expectedTokenKinds.Add(WhitespaceToken);
            }

            expectedTokenKinds.Add(EofToken);

            AssertEx.KindIs(expectedTokenKinds, tokens);

            Assert.True(tokens[0].Position.Line == 0, "A position");
            Assert.True(tokens[2].Position.Line == 1, "B position");
            Assert.True(tokens[4].Position.Line == 2, "C position");
            Assert.True(tokens[6].Position.Line == 3, "D position");
            Assert.True(tokens[8].Position.Line == 5, "E position");
            Assert.True(tokens[10].Position.Line == 7, "F position");
            Assert.True(tokens[^1].Position.Line == 9, "Eof position"); // 9 and not 10, because \r\r\n is only two line breaks
        }
    }
}
