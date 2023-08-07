using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Delta.Slang.Text;

namespace Delta.Slang.Syntax;

internal sealed class Lexer : IDisposable
{
    private struct TokenInfo
    {
        public TokenKind Kind { get; set; }
        public TextSpan Span { get; set; }
        public LinePosition Position { get; set; }
        public object Value { get; set; }
    }

    private readonly DiagnosticCollection diagnostics;
    private readonly SourceText source;

    private int previousLineIndex = 0;
    private int previousColumnIndex = 0;
    private int currentLineIndex = 0;
    private int currentColumnIndex = 0;

    public Lexer(SourceText sourceText)
    {
        source = sourceText;
        TextWindow = new SlidingTextWindow(source);
        diagnostics = new DiagnosticCollection();
    }

    public IEnumerable<IDiagnostic> Diagnostics => diagnostics;

    private SlidingTextWindow TextWindow { get; }

    public void Dispose() => TextWindow.Dispose();

    public IEnumerable<Token> Lex()
    {
        var end = false;
        do
        {
            var tokens = LextNext().Select(info => info.Kind == TokenKind.Eof ?
                Token.MakeEof(info.Span.Start, info.Position) : // Special case: we can't apply source.ToString(span) here
                new Token(info.Kind, info.Span, info.Position, source.ToString(info.Span), info.Value));

            foreach (var tok in tokens)
            {
                yield return tok;
                if (tok.Kind == TokenKind.Eof)
                {
                    end = true;
                    break;
                }
            }
        } while (!end);
    }

    private IEnumerable<TokenInfo> LextNext()
    {
        var info = new TokenInfo();

        AdvanceLinePosition();
        TextWindow.Start();

        // Start scanning the token
        var current = LookAhead();
        switch (current)
        {
            case SlidingTextWindow.InvalidCharacter:
            case '\0':
                info.Kind = TokenKind.Eof;
                Consume();
                break;
            case '+':
                info.Kind = TokenKind.Plus;
                Consume();
                break;
            case '-':
                info.Kind = TokenKind.Minus;
                Consume();
                break;
            case '*':
                info.Kind = TokenKind.Star;
                Consume();
                break;
            case '/':
                LexPotentialComment(ref info);
                break;
            case '(':
                info.Kind = TokenKind.OpenParenthesis;
                Consume();
                break;
            case ')':
                info.Kind = TokenKind.CloseParenthesis;
                Consume();
                break;
            case '{':
                info.Kind = TokenKind.OpenBrace;
                Consume();
                break;
            case '}':
                info.Kind = TokenKind.CloseBrace;
                Consume();
                break;
            case ':':
                info.Kind = TokenKind.Colon;
                Consume();
                break;
            case ',':
                info.Kind = TokenKind.Comma;
                Consume();
                break;
            case ';':
                info.Kind = TokenKind.Semicolon;
                Consume();
                break;
            case '<':
            case '>':
            case '=':
            case '!':
                LexOperatorEndingWithOptionalEqual(current, ref info);
                break;
            case '"':
                return LexStringLiteral();
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                LexNumberLiteral(ref info);
                break;
            default:
                if (IsIdentifierFirstCharacter(current))
                    LexIdentifierOrKeyword(ref info);
                else if (char.IsWhiteSpace(current))
                    LexWhiteSpace(ref info);
                else
                {
                    diagnostics.ReportInvalidCharacter(GetCurrentLinePosition(), GetCurrentSpan(), current);
                    info.Kind = TokenKind.Invalid;
                    Consume();
                }
                break;
        }

        info.Position = GetPreviousLinePosition();
        info.Span = GetCurrentSpan();

        return new[] { info };
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
        info.Kind = TokenKind.Slash;
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

        info.Kind = TokenKind.Comment;
    }

    private void LexCComment(ref TokenInfo info)
    {
        Consume(); // This consumes the second * after the /

        while (true)
        {
            var current = LookAhead();

            if (current is '\0' or SlidingTextWindow.InvalidCharacter)
                break; // prevent infinite loop

            if (ConsumeLineBreakIfAny(current))
                continue;

            // No support for nested comments: we stop at the first */
            if (current == '*')
            {
                Consume();
                var next = LookAhead();
                if (next == '/')
                {
                    Consume();
                    break;
                }
            }

            // All other cases keep looping
            Consume();
        }

        info.Kind = TokenKind.Comment;
    }

    private void LexOperatorEndingWithOptionalEqual(char firstCharacter, ref TokenInfo info)
    {
        Consume();
        var hasAdditionalEqual = false;
        if (LookAhead() == '=')
        {
            Consume();
            hasAdditionalEqual = true;
        }

        info.Kind = firstCharacter switch
        {
            '<' => hasAdditionalEqual ? TokenKind.LowerEqual : TokenKind.Lower,
            '>' => hasAdditionalEqual ? TokenKind.GreaterEqual : TokenKind.Greater,
            '=' => hasAdditionalEqual ? TokenKind.EqualEqual : TokenKind.Equal,
            '!' => hasAdditionalEqual ? TokenKind.ExclamationEqual : TokenKind.Exclamation,
            _ => throw new NotSupportedException("Theoretically, this code is never reached"),
        };
    }

    private IEnumerable<TokenInfo> LexStringLiteral()
    {
        Consume(); // This consumes the initial quote (")
        yield return new TokenInfo
        {
            Kind = TokenKind.DoubleQuote,
            Position = GetPreviousLinePosition(),
            Span = GetCurrentSpan()
        };

        AdvanceLinePosition();
        TextWindow.Start();

        var info = new TokenInfo(); // This is the string content
        var isEscaping = false;
        var buffer = new List<char>();
        while (true)
        {
            var current = LookAhead();
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

        info.Kind = TokenKind.StringLiteral;
        info.Position = GetPreviousLinePosition();
        info.Span = GetCurrentSpan();
        info.Value = new string(buffer.ToArray());
        yield return info;

        AdvanceLinePosition();
        TextWindow.Start();

        Consume(); // This consumes the ending quote (")
        yield return new TokenInfo
        {
            Kind = TokenKind.DoubleQuote,
            Position = GetPreviousLinePosition(),
            Span = GetCurrentSpan()
        };
    }

    private void LexNumberLiteral(ref TokenInfo info)
    {
        while (char.IsDigit(LookAhead()))
            Consume();

        if (LookAhead() == '.')
            LexFloatingPointLiteral(ref info);
        else
        {
            info.Kind = TokenKind.IntLiteral;
            var span = GetCurrentSpan();
            var text = source.ToString(span);
            if (int.TryParse(text, out var value))
                info.Value = value;
            else
                diagnostics.ReportInvalidNumber(GetCurrentLinePosition(), GetCurrentSpan(), text);
        }
    }

    private void LexFloatingPointLiteral(ref TokenInfo info)
    {
        Consume(); // This consumes the '.' character

        while (char.IsDigit(LookAhead()))
            Consume();

        info.Kind = TokenKind.DoubleLiteral;
        var span = GetCurrentSpan();
        var text = source.ToString(span);
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            info.Value = value;
        else
            diagnostics.ReportInvalidNumber(GetCurrentLinePosition(), GetCurrentSpan(), text);
    }

    private void LexWhiteSpace(ref TokenInfo info)
    {
        while (true)
        {
            var current = LookAhead();
            if (ConsumeLineBreakIfAny(current))
                continue;

            if (char.IsWhiteSpace(current))
            {
                Consume();
                continue;
            }

            // All other cases
            break;
        }

        info.Kind = TokenKind.Whitespace;
    }

    private bool ConsumeLineBreakIfAny(char current)
    {
        if (current == '\r')
        {
            currentLineIndex++;
            Consume();

            var next = LookAhead();
            if (next == '\n')
                Consume();

            currentColumnIndex = 0;
            return true;
        }

        if (current == '\n')
        {
            currentLineIndex++;
            Consume();

            currentColumnIndex = 0;
            return true;
        }

        return false;
    }

    private void LexIdentifierOrKeyword(ref TokenInfo info)
    {
        while (IsIdentifierCharacter(LookAhead()))
            Consume();

        var text = source.ToString(GetCurrentSpan());
        info.Kind = TokenKindConversions.GetIdentifierOrKeyword(text);
    }

    private char LookAhead() => TextWindow.PeekChar();
    private void Consume()
    {
        TextWindow.AdvanceChar();
        currentColumnIndex++;
    }

    private void AdvanceLinePosition()
    {
        previousColumnIndex = currentColumnIndex;
        previousLineIndex = currentLineIndex;
    }

    private TextSpan GetCurrentSpan() => new(TextWindow.LexemeStartPosition, TextWindow.Width);
    private LinePosition GetPreviousLinePosition() => new(previousLineIndex, previousColumnIndex);
    private LinePosition GetCurrentLinePosition() => new(currentLineIndex, currentColumnIndex);

    private static bool IsIdentifierFirstCharacter(char c) => c == '_' || char.IsLetter(c);
    private static bool IsIdentifierCharacter(char c) => IsIdentifierFirstCharacter(c) || char.IsDigit(c);
}
