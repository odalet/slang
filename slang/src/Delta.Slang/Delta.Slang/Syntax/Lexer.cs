using System;
using System.Collections.Generic;
using System.Linq;
using Delta.Slang.Text;

namespace Delta.Slang.Syntax
{
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

        public IEnumerable<(int, Token)> LexWithLineNumbers()
        {
            Token token;
            do
            {
                var info = new TokenInfo();
                LexNext(ref info);

                token = info.Kind == TokenKind.Eof ?
                    Token.MakeEof(info.Span.Start, info.Position) : // Special case: we can't apply source.ToString(span) here
                    new Token(info.Kind, info.Span, info.Position, source.ToString(info.Span), info.Value);

                yield return (previousLineIndex, token);
            } while (token.Kind != TokenKind.Eof);
        }

        public IEnumerable<Token> Lex() => LexWithLineNumbers().Select(((int l, Token t) x) => x.t);

        private void LexNext(ref TokenInfo info)
        {
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
                    LexStringLiteral(ref info);
                    break;
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
            Consume();
        }

        private void LexCppComment(ref TokenInfo info)
        {
            Consume(); // This consumes the second /
            while (true)
            {
                var current = LookAhead();
                if (current == '\r' || current == '\n' || current == '\0' || current == SlidingTextWindow.InvalidCharacter)
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

                if (current == '\0' || current == SlidingTextWindow.InvalidCharacter)
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

            switch (firstCharacter)
            {
                case '<': info.Kind = hasAdditionalEqual ? TokenKind.LowerEqual : TokenKind.Lower; break;
                case '>': info.Kind = hasAdditionalEqual ? TokenKind.GreaterEqual : TokenKind.Greater; break;
                case '=': info.Kind = hasAdditionalEqual ? TokenKind.EqualEqual : TokenKind.Equal; break;
                case '!': info.Kind = hasAdditionalEqual ? TokenKind.ExclamationEqual : TokenKind.Exclamation; break;
                default: throw new NotSupportedException("Theoretically, this code is never reached");
            }
        }

        private void LexStringLiteral(ref TokenInfo info)
        {
            Consume(); // This consumes the initial quote (")

            var isEscaping = false;
            while (true)
            {
                var current = LookAhead();
                if (current == '\\' && !isEscaping)
                {
                    isEscaping = true;
                    Consume();
                    continue;
                }

                if (current == '\"' && !isEscaping)
                {
                    Consume();
                    break;
                }

                Consume();
                isEscaping = false; // Not escaping any more...
            }

            info.Kind = TokenKind.StringLiteral;
        }

        private void LexNumberLiteral(ref TokenInfo info)
        {
            while (char.IsDigit(LookAhead()))
                Consume();

            info.Kind = TokenKind.NumberLiteral;
            var span = GetCurrentSpan();
            var text = source.ToString(span);
            if (int.TryParse(text, out var value))
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
            info.Kind = GetIdentifierOrKeyword(text);
        }

        private bool IsIdentifierFirstCharacter(char c) => c == '_' || char.IsLetter(c);
        private bool IsIdentifierCharacter(char c) => IsIdentifierFirstCharacter(c) || char.IsDigit(c);

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

        private TextSpan GetCurrentSpan() => new TextSpan(TextWindow.LexemeStartPosition, TextWindow.Width);
        private LinePosition GetPreviousLinePosition() => new LinePosition(previousLineIndex, previousColumnIndex);
        private LinePosition GetCurrentLinePosition() => new LinePosition(currentLineIndex, currentColumnIndex);
        private TokenKind GetIdentifierOrKeyword(string text)
        {
            switch (text)
            {
                case "fun": return TokenKind.FunKeyword;
                case "var": return TokenKind.VarKeyword;
                case "return": return TokenKind.ReturnKeyword;
                case "true": return TokenKind.TrueKeyword;
                case "false": return TokenKind.FalseKeyword;
                default: return TokenKind.Identifier;
            }
        }
    }
}
