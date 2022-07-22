using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;
    using static LexerDiagnostic.ErrorCode;

    [ExcludeFromCodeCoverage]
    public class LexCommentTests
    {
        [Fact]
        public void Cpp_comments_are_not_aggregated()
        {
            var source = "// Comment Line 1\r\n// Comment Line 2";
            var (tokens, _) = Helper.Lex(source);

            AssertEx.KindIs(new[] { CommentToken, WhitespaceToken, CommentToken, EofToken }, tokens);
        }

        [Fact]
        public void C_comments_can_span_multiple_lines()
        {
            var source = "/* Comment Line 1\r\nComment Line 2 */";
            var (tokens, _) = Helper.Lex(source);

            AssertEx.KindIs(new[] { CommentToken, EofToken }, tokens);
        }

        [Theory]
        [InlineData("/* Comment Line 1\r\nComment Line 2 ")]
        [InlineData("/* Comment Line 1\r\nComment Line 2 *")]
        public void Unterminated_C_comment_generates_a_diagnostic(string source)
        {
            var (tokens, diags) = Helper.Lex(source);
            AssertEx.KindIs(new[] { CommentToken, EofToken }, tokens);
            Assert.Equal(UnterminatedComment.ToId(), diags[0].Id);
        }

        [Fact]
        public void Nested_C_comments_are_not_supported_but_generate_a_diagnostic()
        {
            var source = "/* Comment /* Nested comment */ outer comment */";
            var (_, diags) = Helper.Lex(source);
            Assert.Equal(UnexpectedEndOfComment.ToId(), diags[0].Id);
        }
    }
}
