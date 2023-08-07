using System.Globalization;

namespace Slang.CodeAnalysis.Syntax;

using static SyntaxKind;

partial class Lexer
{
    private void LexNumberLiteral(ref TokenInfo info)
    {
        while (IsDigit(LookAhead()))
            Consume();

        // A digit after the dot means we are looking at a decimal separator
        if (LookAhead() == '.' && IsDigit(LookAhead(1)))
        {
            Consume(); // This consumes the '.' character

            while (IsDigit(LookAhead()))
                Consume();

            MakeFloatLiteralToken(ref info);
            return;
        }

        // Otherwise, don't consume the dot (it will be consumed by the
        // general lexing loop) and build an integer
        MakeIntegerLiteralToken(ref info);
    }

    private void MakeIntegerLiteralToken(ref TokenInfo info)
    {
        info.Kind = IntegerLiteralToken;
        var span = GetCurrentSpan();
        var text = source.ToString(span);
        if (int.TryParse(text, out var value))
            info.Value = value;
        else
            diagnostics.ReportInvalidInteger(GetCurrentLinePosition(), GetCurrentSpan(), text);
    }

    private void MakeFloatLiteralToken(ref TokenInfo info)
    {
        info.Kind = FloatLiteralToken;
        var span = GetCurrentSpan();
        var text = source.ToString(span);

        // Given the input (digits and 1 point), there's no way we can fail at parsing here
        info.Value = double.Parse(text, NumberStyles.Any, CultureInfo.InvariantCulture);
    }
}
