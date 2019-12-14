using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Delta.Slang.Text;

namespace Delta.Slang.Syntax
{
    internal sealed class Lexer : IDisposable
    {
        private struct TokenInfo
        {
            public TokenKind Kind { get; set; }
        }

        private readonly ICollection<Diagnostic> diagnostics;
        private readonly SourceText source;
        private int currentLineIndex = 0;

        public Lexer(SourceText sourceText)
        {
            source = sourceText;
            TextWindow = new SlidingTextWindow(source);
            diagnostics = new List<Diagnostic>();
            ////lineLexer = new LineLexer();
        }

        public IEnumerable<IDiagnostic> Diagnostics => diagnostics;

        private SlidingTextWindow TextWindow { get; }

        public void Dispose() => TextWindow.Dispose();

        public IEnumerable<(int, Token)> Lex()
        {
            Token token;
            do
            {
                var info = new TokenInfo();
                LexNext(ref info);
                var span = GetCurrentSpan();

                token = info.Kind == TokenKind.Eof ?
                    Token.MakeEof(span.Start) : // Special case: we can't apply source.ToString(span) here
                    new Token(info.Kind, span, source.ToString(span));

                yield return (currentLineIndex, token);
            } while (token.Kind != TokenKind.Eof);
        }

        private void LexNext(ref TokenInfo info)
        {
            TextWindow.Start();

            // Start scanning the token
            var current = LookAhead();
            switch (current)
            {
                case SlidingTextWindow.InvalidCharacter:
                case '\0':
                    info.Kind = TokenKind.Eof;
                    break;
                case '\"':
                    LexStringLiteral(ref info);
                    break;
                case '(':
                    info.Kind = TokenKind.OpenParenthesis;
                    Consume();
                    break;
                case ')':
                    info.Kind = TokenKind.CloseParenthesis;
                    Consume();
                    break;
                case ';':
                    info.Kind = TokenKind.Semicolon;
                    Consume();
                    break;
                default:
                    if (char.IsLetter(current))
                        LexIdentifierOrKeyword(ref info);
                    else if (char.IsWhiteSpace(current))
                        LexWhiteSpace(ref info);
                    else
                    {
                        diagnostics.Add(new LexerError(currentLineIndex, GetCurrentSpan(), $"Encountered invalid character: {current}"));
                        Consume();
                    }
                    break;
            }
        }

        private void LexStringLiteral(ref TokenInfo info)
        {
            var quoteCharacter = LookAhead();
            Consume();

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

        private void LexWhiteSpace(ref TokenInfo info)
        {
            while (true)
            {
                var current = LookAhead();
                if (current == '\r')
                {
                    currentLineIndex++;
                    Consume();

                    var next = LookAhead();
                    if (next == '\n')
                        Consume();

                    continue;
                }

                if (current == '\n')
                {
                    currentLineIndex++;
                    Consume();
                    continue;
                }

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

        private void LexIdentifierOrKeyword(ref TokenInfo info)
        {
            while (char.IsLetter(LookAhead()))
                Consume();

            info.Kind = TokenKind.Identifier;
        }

        private char LookAhead() => TextWindow.PeekChar();
        private char LookAhead(int n) => n <= 1 ? LookAhead() : TextWindow.PeekChar(n - 1);
        private void Consume() => TextWindow.AdvanceChar();
        private void Consume(int n)
        {
            if (n <= 1)
                TextWindow.AdvanceChar();
            else
                TextWindow.AdvanceChar(n);
        }

        private TextSpan GetCurrentSpan() => new TextSpan(TextWindow.LexemeStartPosition, TextWindow.Width);
    }


    internal sealed class _Lexer
    {
        private enum LexMode
        {
            Default,
            InsideStringLiteral,
            InsideComment
        }

        ////private readonly LineLexer lineLexer;
        private readonly TextReader input;
        ////private int currentLine;

        private int position;
        private int currentLineIndex;
        ////private string line;
        private LexMode mode;
        private readonly ICollection<Diagnostic> diagnostics;

        public _Lexer(TextReader reader)
        {
            input = reader ?? throw new ArgumentNullException(nameof(reader));
            diagnostics = new List<Diagnostic>();
            ////lineLexer = new LineLexer();
        }

        public IEnumerable<(int, Token)> Lex(ICollection<Diagnostic> diagnostics)
        {
            Token token;
            Token invalidToken = null;
            do
            {
                token = LexNext();
                yield return (currentLineIndex, token);
            } while (token.Kind != TokenKind.Eof);

            ////string line = null;
            ////while ((line = input.ReadLine()) != null)
            ////{
            ////    currentLineIndex++;
            ////    foreach (var token in LexLine(line, diagnostics))
            ////        yield return (currentLineIndex, token);
            ////}

            ////yield return (currentLineIndex, Token.MakeEof(0));
        }

        private Token LexNext()
        {
            var start = position;
            var kind = TokenKind.Invalid;
            var text = "";

            var current = LookAhead();
            switch (current)
            {
                case '\0':
                    kind = TokenKind.Eof;
                    break;
                case '"':
                    kind = TokenKind.DoubleQuote;
                    text += Consume();
                    if (mode == LexMode.Default)
                    {
                        mode = LexMode.InsideStringLiteral;
                    }

                    break;
                case '(':
                    kind = TokenKind.OpenParenthesis;
                    text += Consume();
                    break;
                case ')':
                    kind = TokenKind.CloseParenthesis;
                    text += Consume();
                    break;
                case ';':
                    kind = TokenKind.Semicolon;
                    text += Consume();
                    break;
                default:
                    if (char.IsLetter(current))
                        return LexIdentifierOrKeyword();
                    else if (char.IsWhiteSpace(current))
                        return LexWhiteSpace();
                    else
                    {
                        text += Consume();
                        diagnostics.Add(new LexerError(currentLineIndex, new TextSpan(start, text.Length), $"Encountered invalid character: {current}"));
                    }
                    break;
            }

            return new Token(kind, new TextSpan(start, text.Length), text);
        }

        private Token LexWhiteSpace()
        {
            var start = position;
            var builder = new StringBuilder();
            while (char.IsWhiteSpace(LookAhead()))
                builder.Append(Consume());

            return new Token(TokenKind.Whitespace, new TextSpan(start, builder.Length), builder.ToString());
        }

        private Token LexIdentifierOrKeyword()
        {
            var start = position;
            var builder = new StringBuilder();
            while (char.IsLetter(LookAhead()))
                builder.Append(Consume());

            return new Token(TokenKind.Identifier, new TextSpan(start, builder.Length), builder.ToString());
        }

        private char LookAhead()
        {
            var value = input.Peek();
            return value == -1 ? '\0' : (char)value;
        }

        private char Consume()
        {
            var value = input.Read();
            position++;
            return value == -1 ? '\0' : (char)value;
        }

        ////private IEnumerable<Token> LexLine(string line, ICollection<Diagnostic> diagnostics)
        ////{ 
        ////    Token token;
        ////    Token invalidToken = null;
        ////    do
        ////    {
        ////        token = LexNext(line);

        ////        if (token.Kind == TokenKind.Invalid)
        ////        {
        ////            if (invalidToken == null)
        ////                invalidToken = token;
        ////            else // Merge the tokens, so as not to have one invalid token per character
        ////            {
        ////                var span = new TextSpan(invalidToken.Span.Start, invalidToken.Span.Length + token.Span.Length);
        ////                invalidToken = new Token(TokenKind.Invalid, span, invalidToken.Text + token.Text);
        ////            }
        ////        }
        ////        else
        ////        {
        ////            if (invalidToken != null)
        ////            {
        ////                diagnostics.Add(new LexerError(
        ////                    currentLineIndex, invalidToken.Span, $"Invalid Token: '{invalidToken.Text}'"));
        ////                yield return invalidToken;
        ////            }

        ////            invalidToken = null;
        ////            yield return  token;
        ////        }

        ////    } while (token.Kind != TokenKind.Eol);
        ////}

        ////private Token LexNext(string line)
        ////{
        ////    if (position >= line.Length) // should never happen...
        ////        return Token.MakeEol(line.Length);

        ////    var current = Peek();

        ////    ////if (current == '(') // Comment
        ////    ////    return LexComment(position);

        ////    ////if (current == ';') // Reprap comment; see https://reprap.org/wiki/G-code#Comments
        ////    ////    return LexReprapComment(position);

        ////    ////var isStar = current == '*';
        ////    ////if (current == 'n' || current == 'N' || isStar) // LineNumber or Star
        ////    ////    return LexLineNumberOrStar(position, isStar);

        ////    ////if (char.IsLetter(current)) // Word
        ////    ////    return LexWord(position);

        ////    return LexInvalidToken(position);
        ////}

        ////private IEnumerable<Token> LexLine(string line, ICollection<Diagnostic> diagnostics)
        ////{
        ////    lineLexer.Initialize(currentLine, line, diagnostics);
        ////    return lineLexer.Lex();
        ////}

        ////private Token LexInvalidToken(int start)
        ////{
        ////    Consume();
        ////    var length = position - start;
        ////    return new Token(TokenKind.Invalid, new TextSpan(start, length), line.Substring(start, length));
        ////}

        ////private char Peek(int offset)
        ////{
        ////    var index = position + offset;

        ////    if (index >= line.Length)
        ////        return '\0';

        ////    return line[index];
        ////}

        ////private char Peek() => position >= line.Length ? '\0' : line[position];

        ////private char LookAhead()
        ////{
        ////    if (mode != LexMode.Default)
        ////        throw new InvalidOperationException("Look Ahead is only supported in Default Lex Mode");

        ////    var offset = 1;
        ////    var index = position + offset;
        ////    while (true)
        ////    {
        ////        if (index >= line.Length)
        ////            return '\0';
        ////        if (!char.IsWhiteSpace(line[index]))
        ////            return line[index];
        ////        index++;
        ////    }
        ////}

        ////private void Consume()
        ////{
        ////    position++;
        ////    if (mode == LexMode.Default)
        ////    {
        ////        while (char.IsWhiteSpace(Peek()))
        ////            position++;
        ////    }
        ////}
    }
}
