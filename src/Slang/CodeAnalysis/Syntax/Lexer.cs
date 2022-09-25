using System;
using System.Collections.Generic;
using System.Linq;
using Slang.CodeAnalysis.Text;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;
    using static TokenCategory;

    public sealed partial class Lexer
    {
        private struct TokenInfo
        {
            public SyntaxKind Kind { get; set; }
            public TextSpan Span { get; set; }
            public LinePosition Position { get; set; }
            public object Value { get; set; }

            private bool IsTrivia => Kind is WhitespaceToken or DoubleQuoteToken or CommentToken;

            public Token ToToken(SourceText source) =>
                new(Kind, IsTrivia ? Trivia : Terminal, Span, Position, source.ToString(Span), Value);
        }

        private readonly SourceText source;
        private readonly IDiagnosticSink diagnostics;
        private readonly SlidingTextWindow window;

        private (int line, int column) previousPosition;
        private (int line, int column) currentPosition;

        public Lexer(SourceText sourceText, IDiagnosticSink diagnosticSink)
        {
            source = sourceText;
            diagnostics = diagnosticSink;
            window = new SlidingTextWindow(source);
        }

        public IEnumerable<Token> Lex()
        {
            try
            {
                return LexSource();
            }
            catch (Exception ex)
            {
                diagnostics.ReportLexerException(ex);
                return Array.Empty<Token>();
            }
        }

        private IEnumerable<Token> LexSource()
        {
            var end = false;
            do
            {
                var tokens = LexNext().Select(info =>
                    info.Kind == EofToken
                    ? Token.MakeEof(info.Span.Start, info.Position) // Special case: we can't apply source.ToString(span) here
                    : info.ToToken(source));

                foreach (var tok in tokens)
                {
                    yield return tok;
                    if (tok.Kind == EofToken)
                    {
                        end = true;
                        break;
                    }
                }
            } while (!end);
        }

        private IEnumerable<TokenInfo> LexNext()
        {
            var info = new TokenInfo();

            void setAndConsume(SyntaxKind kind)
            {
                info.Kind = kind;
                Consume();
            }

            // Start scanning the token
            StartNewLexeme();

            var current = LookAhead();
            switch (current)
            {
                case '\0' or SlidingTextWindow.InvalidCharacter: setAndConsume(EofToken); break;
                case '+': setAndConsume(PlusToken); break;
                case '-': setAndConsume(MinusToken); break;
                case '*': LexPotentialEndOfComment(ref info); break;
                case '/': LexPotentialComment(ref info); break;
                case '(': setAndConsume(LeftParenToken); break;
                case ')': setAndConsume(RightParenToken); break;
                case '{': setAndConsume(LeftBraceToken); break;
                case '}': setAndConsume(RightBraceToken); break;
                case '.': setAndConsume(DotToken); break;
                case ',': setAndConsume(CommaToken); break;
                case ':': setAndConsume(ColonToken); break;
                case ';': setAndConsume(SemicolonToken); break;
                case '&' or '|': LexLogicalOperator(current, ref info); break;
                case '<' or '>' or '=' or '!': LexOperatorEndingWithOptionalEqual(current, ref info); break;
                case '"':
                    return LexStringLiteral();
                default:
                    if (IsDigit(current))
                        LexNumberLiteral(ref info);
                    else if (IsIdentifierFirstCharacter(current))
                        LexIdentifierOrReservedWord(ref info);
                    else if (IsWhitespaceOrLineBreak(current))
                        LexWhiteSpace(ref info);
                    else
                    {
                        diagnostics.ReportInvalidCharacter(GetCurrentLinePosition(), GetCurrentSpan(), current);
                        info.Kind = InvalidToken;
                        Consume();
                    }

                    break;
            }

            info.Position = GetPreviousLinePosition();
            info.Span = GetCurrentSpan();

            return new[] { info };
        }

        private char LookAhead() => window.PeekChar();
        private char LookAhead(int n) => window.PeekChar(n);

        private void Consume()
        {
            window.AdvanceChar();
            currentPosition.column++;
        }

        private void StartNewLexeme()
        {
            previousPosition = currentPosition;
            window.Start();
        }

        private TextSpan GetCurrentSpan() => new(window.LexemeStartPosition, window.Width);
        private LinePosition GetPreviousLinePosition() => new(previousPosition.line, previousPosition.column);
        private LinePosition GetCurrentLinePosition() => new(currentPosition.line, currentPosition.column);

        // Do not use char.IsDigit because it includes lots of other
        // characters (everything that is a number in many scripts (arabic, mongolian...)
        static bool IsDigit(char c) => c is >= '0' and <= '9';

        // Do not use char.IsWhiteSpace because it includes several weird characters
        // We only accept ' ' and \t as whitespaces and '\r', '\n' as line breaks.
        private static bool IsWhitespaceOrLineBreak(char c) => c is ' ' or '\t' or '\r' or '\n';

        // NB: in the 2 methods below, we allow all characters in the corresponding unicode
        // categories, not only the basic English/Latin ones.
        private static bool IsIdentifierFirstCharacter(char c) => c == '_' || char.IsLetter(c);
        private static bool IsIdentifierCharacter(char c) => IsIdentifierFirstCharacter(c) || char.IsDigit(c);

        private static bool IsEof(char c) => c == SlidingTextWindow.InvalidCharacter;
    }
}
