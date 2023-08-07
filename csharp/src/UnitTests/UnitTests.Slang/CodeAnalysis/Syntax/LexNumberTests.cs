using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Slang.CodeAnalysis.Syntax;

using static SyntaxKind;
using static LexerDiagnostic.ErrorCode;

[ExcludeFromCodeCoverage]
public class LexNumberTests
{
    [Fact]
    public void Integer_generates_an_integer_token_with_value()
    {
        var source = "42";
        var (tokens, _) = Helper.Lex(source);

        AssertEx.KindIs(new[] { IntegerLiteralToken, EofToken }, tokens);
        Assert.Equal(42, tokens[0].Value);
    }

    [Fact]
    public void Float_generates_an_float_token_with_value()
    {
        var source = "3.14";
        var (tokens, _) = Helper.Lex(source);

        AssertEx.KindIs(new[] { FloatLiteralToken, EofToken }, tokens);
        Assert.Equal(3.14, tokens[0].Value);
    }

    [Fact]
    public void Number_then_point_then_text_is_integer_and_dot_token()
    {
        var source = "42.foo";
        var (tokens, _) = Helper.Lex(source);

        AssertEx.KindIs(new[] { IntegerLiteralToken, DotToken, IdentifierToken, EofToken }, tokens);
    }

    [Fact]
    public void Number_then_point_then_number_then_point_then_text_is_float_and_dot_token()
    {
        var source = "3.14.foo";
        var (tokens, _) = Helper.Lex(source);

        AssertEx.KindIs(new[] { FloatLiteralToken, DotToken, IdentifierToken, EofToken }, tokens);
    }

    [Fact]
    public void Too_long_integer_generates_an_invalid_integer_diagnostic()
    {
        var source = "123456789012345678901234567890";
        var (tokens, diags) = Helper.Lex(source);

        AssertEx.KindIs(new[] { IntegerLiteralToken, EofToken }, tokens);
        Assert.Null(tokens[0].Value);
        Assert.Equal(InvalidInteger.ToId(), diags[0].Id);
    }
}
