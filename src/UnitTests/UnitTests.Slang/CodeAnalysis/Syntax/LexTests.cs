using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;
    using static LexerDiagnostic.ErrorCode;

    [ExcludeFromCodeCoverage]
    public class LexTests
    {
        // What happens when the input starts with a UTF-8 BOM
        [Fact]
        public void Bom_is_ignored()
        {
            var filename = Helper.Resolve("bom.sl");
            var bytes = File.ReadAllBytes(filename);
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            _ = bytes[0..3].Should().BeEquivalentTo(bom, "Ensuring the input file starts with a UTF-8 BOM");

            var (tokens, _) = Helper.LexFile(filename);
            AssertEx.KindIs(new[] { IdentifierToken, EofToken }, tokens);
        }

        [Fact]
        public void Invalid_character_raises_a_diagnostic()
        {
            var source = "~"; // Beware, may become valid in the future
            var (tokens, diags) = Helper.Lex(source);

            AssertEx.KindIs(new[] { InvalidToken, EofToken }, tokens);
            Assert.Equal(InvalidCharacter.ToId(), diags[0].Id);
        }
    }
}
