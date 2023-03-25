using System.Collections.Generic;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;

    partial class Lexer
    {
        private IEnumerable<TokenInfo> LexStringLiteral()
        {
            Consume(); // This consumes the initial quote (")
            yield return new TokenInfo
            {
                Kind = DoubleQuoteToken,
                Position = GetPreviousLinePosition(),
                Span = GetCurrentSpan()
            };

            StartNewLexeme();

            var info = new TokenInfo(); // This is the string content
            var isEscaping = false;
            var buffer = new List<char>();
            var unterminated = false;
            while (true)
            {
                var current = LookAhead();

                if (IsEof(current))
                {
                    // We shouldn't be here: it means the string wasn't terminated with a quote
                    unterminated = true;
                    break;
                }

                if (current == '\"' && !isEscaping)
                    break;

                buffer.Add(current);

                if (current == '\\' && !isEscaping)
                {
                    isEscaping = true;
                    Consume();
                    continue;
                }

                Consume();
                isEscaping = false; // Not escaping any more...
            }

            info.Kind = StringLiteralToken;
            info.Position = GetPreviousLinePosition();
            info.Span = GetCurrentSpan();
            info.Value = new string(buffer.ToArray());
            yield return info;

            if (unterminated)
            {
                diagnostics.ReportUnterminatedString(GetCurrentLinePosition(), GetCurrentSpan());
                yield break;
            }

            StartNewLexeme();
            Consume(); // This consumes the ending quote (")
            yield return new TokenInfo
            {
                Kind = DoubleQuoteToken,
                Position = GetPreviousLinePosition(),
                Span = GetCurrentSpan()
            };
        }
    }
}
