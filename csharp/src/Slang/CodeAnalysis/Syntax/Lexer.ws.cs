namespace Slang.CodeAnalysis.Syntax;

using static SyntaxKind;

partial class Lexer
{
    private void LexWhiteSpace(ref TokenInfo info)
    {
        while (true)
        {
            var current = LookAhead();
            if (ConsumeLineBreakIfAny(current))
                continue;

            if (current is ' ' or '\t')
            {
                Consume();
                continue;
            }

            // All other cases
            break;
        }

        info.Kind = WhitespaceToken;
    }

    private bool ConsumeLineBreakIfAny(char current)
    {
        if (current == '\r')
        {
            currentPosition.line++;
            Consume();

            var next = LookAhead();
            if (next == '\n')
                Consume();

            currentPosition.column = 0;
            return true;
        }

        if (current == '\n')
        {
            currentPosition.line++;
            Consume();

            currentPosition.column = 0;
            return true;
        }

        return false;
    }
}
