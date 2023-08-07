namespace Slang.CodeAnalysis.Syntax;

using static SyntaxKind;

partial class Lexer
{
    private void LexPotentialEndOfComment(ref TokenInfo info)
    {
        Consume(); // This consumes the initial *
        
        var current = LookAhead();
        if (current == '/') // This is an unexpected end of C comment
        {
            info.Kind = CommentToken;
            Consume();
            diagnostics.ReportUnexpectedEndOfComment(GetCurrentLinePosition(), GetCurrentSpan());
        }
        else info.Kind = StarToken; // OK, this is the '*' operator
    }

    private void LexPotentialComment(ref TokenInfo info)
    {
        Consume(); // This consumes the initial /

        var current = LookAhead();
        if (current == '/') // Single line comment
        {
            LexCppComment(ref info);
            return;
        }

        if (current == '*') // Multi line comment
        {
            LexCComment(ref info);
            return;
        }

        // Default is the / operator
        info.Kind = SyntaxKind.SlashToken;
        // Do not consume: it was already done at the beginning of the method!
    }

    private void LexCppComment(ref TokenInfo info)
    {
        Consume(); // This consumes the second /
        while (true)
        {
            var current = LookAhead();
            if (current is '\r' or '\n' or '\0' or SlidingTextWindow.InvalidCharacter)
                break;
            Consume();
        }

        info.Kind = SyntaxKind.CommentToken;
    }

    private void LexCComment(ref TokenInfo info)
    {
        Consume(); // This consumes the second * after the /

        while (true)
        {
            var current = LookAhead();

            if (current is '\0' or SlidingTextWindow.InvalidCharacter)
            {
                // Unterminated comment!
                diagnostics.ReportUnterminatedComment(GetCurrentLinePosition(), GetCurrentSpan());
                break; // prevent infinite loop
            }

            if (ConsumeLineBreakIfAny(current))
                continue;

            // No support (yet) for nested comments: we stop at the first */
            if (current == '*')
            {
                Consume();
                var next = LookAhead();
                if (next == '/')
                {
                    Consume();
                    break;
                }
                else if (next is '\0' or SlidingTextWindow.InvalidCharacter)
                {
                    diagnostics.ReportUnterminatedComment(GetCurrentLinePosition(), GetCurrentSpan());
                    break;
                }
            }

            Consume(); // All other cases: keep looping
        }

        info.Kind = SyntaxKind.CommentToken;
    }
}
