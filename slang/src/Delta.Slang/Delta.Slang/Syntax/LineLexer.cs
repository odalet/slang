////////using System;
////////using System.Collections.Generic;
////////using System.Text;

////////namespace Delta.Slang.Syntax
////////{
////////    // This is the real lexer: it tokenizes one line of G-Code
////////    // Inspired by https://github.com/terrajobst/minsk/blob/master/Minsk/CodeAnalysis/Syntax/Lexer.cs
////////    internal sealed class LineLexer
////////    {
////////        private enum LexMode
////////        {
////////            Default,
////////            InsideComment
////////        }

////////        private int position;
////////        private int currentLineIndex;
////////        private string line;
////////        private LexMode mode;
////////        private ICollection<Diagnostic> diagnostics;

////////        // We provide this method to initialize/reset the lexer so as to minimize instances creation.
////////        public void Initialize(int lineIndex, string lineText, ICollection<Diagnostic> diagnosticsCollection)
////////        {
////////            mode = LexMode.Default;
////////            position = 0;
////////            currentLineIndex = lineIndex;
////////            line = lineText ?? "";
////////            diagnostics = diagnosticsCollection ?? new List<Diagnostic>();
////////        }

////////        public IEnumerable<Token> Lex()
////////        {
////////            Token token;
////////            Token invalidToken = null;
////////            do
////////            {
////////                token = LexNext();

////////                if (token.Kind == TokenKind.Invalid)
////////                {
////////                    if (invalidToken == null)
////////                        invalidToken = token;
////////                    else // Merge the tokens, so as not to have one invalid token per character
////////                    {
////////                        var span = new TextSpan(invalidToken.Span.Start, invalidToken.Span.Length + token.Span.Length);
////////                        invalidToken = new Token(TokenKind.Invalid, span, invalidToken.Text + token.Text);
////////                    }
////////                }
////////                else
////////                {
////////                    if (invalidToken != null)
////////                    {
////////                        diagnostics.Add(new LexerError(
////////                            currentLineIndex, invalidToken.Span, $"Invalid Token: '{invalidToken.Text}'"));
////////                        yield return invalidToken;
////////                    }

////////                    invalidToken = null;
////////                    yield return token;
////////                }

////////            } while (token.Kind != TokenKind.Eol);

////////            if (invalidToken != null)
////////            {
////////                diagnostics.Add(new LexerError(
////////                    currentLineIndex, invalidToken.Span, $"Invalid Token: '{invalidToken.Text}'"));
////////                yield return invalidToken;
////////            }
////////        }

////////        private Token LexNext()
////////        {
////////            if (position >= line.Length) // should never happen...
////////                return Token.MakeEol(line.Length);

////////            var current = Peek();

////////            if (current == '(') // Comment
////////                return LexComment(position);

////////            if (current == ';') // Reprap comment; see https://reprap.org/wiki/G-code#Comments
////////                return LexReprapComment(position);

////////            var isStar = current == '*';
////////            if (current == 'n' || current == 'N' || isStar) // LineNumber or Star
////////                return LexLineNumberOrStar(position, isStar);

////////            if (char.IsLetter(current)) // Word
////////                return LexWord(position);

////////            return LexInvalidToken(position);
////////        }

////////        private Token LexComment(int start)
////////        {
////////            mode = LexMode.InsideComment;
////////            while (Peek() != ')')
////////                Consume();
////////            mode = LexMode.Default;
////////            Consume(); // Consume the closing parenthesis and eventually folling whitespaces           

////////            var length = position - start;
////////            var text = line.Substring(start, length);
////////            var temp = text.Trim();
////////            var normalized = temp.Substring(1, temp.Length - 2).Trim();
////////            return new Token(TokenKind.Comment, new TextSpan(start, length), text, normalized);
////////        }

////////        private Token LexReprapComment(int start)
////////        {
////////            ConsumeToEnd();
////////            var text = line.Substring(start);
////////            var normalized = text.Substring(1).Trim();
////////            return new Token(TokenKind.Comment, new TextSpan(start, line.Length - start), text, normalized);
////////        }

////////        private Token LexLineNumberOrStar(int start, bool isStar)
////////        {
////////            var next = LookAhead();
////////            if (!IsUnsignedIntegerPart(next))
////////            {
////////                Consume();
////////                return new Token(TokenKind.Invalid, new TextSpan(start, 1), line.Substring(start, 1));
////////            }

////////            var builder = new StringBuilder();
////////            builder.Append(Peek());
////////            Consume();

////////            while (IsUnsignedIntegerPart(Peek()))
////////            {
////////                builder.Append(Peek());
////////                Consume();
////////            }

////////            var normalized = NormalizeWordOrLineNumberOrStar(builder);
////////            return new Token(isStar ? TokenKind.Star : TokenKind.LineNumber, new TextSpan(start, position - start), builder.ToString(), normalized);
////////        }

////////        private Token LexWord(int start)
////////        {
////////            var next = LookAhead();
////////            if (!IsRealPart(next))
////////            {
////////                Consume();
////////                return new Token(TokenKind.Invalid, new TextSpan(start, 1), line.Substring(start, 1));
////////            }

////////            var builder = new StringBuilder();
////////            builder.Append(Peek());
////////            Consume();

////////            while (IsRealPart(Peek()))
////////            {
////////                builder.Append(Peek());
////////                Consume();
////////            }

////////            var normalized = NormalizeWordOrLineNumberOrStar(builder);
////////            return new Token(TokenKind.Word, new TextSpan(start, position - start), builder.ToString(), normalized);
////////        }

////////        private Token LexInvalidToken(int start)
////////        {
////////            Consume();
////////            var length = position - start;
////////            return new Token(TokenKind.Invalid, new TextSpan(start, length), line.Substring(start, length));
////////        }

////////        private char Peek() => position >= line.Length ? '\0' : line[position];

////////        private char LookAhead()
////////        {
////////            if (mode != LexMode.Default)
////////                throw new InvalidOperationException("Look Ahead is only supported in Default Lex Mode");

////////            var offset = 1;
////////            var index = position + offset;
////////            while (true)
////////            {
////////                if (index >= line.Length)
////////                    return '\0';
////////                if (!char.IsWhiteSpace(line[index]))
////////                    return line[index];
////////                index++;
////////            }
////////        }

////////        private char Peek(int offset)
////////        {
////////            var index = position + offset;

////////            if (index >= line.Length)
////////                return '\0';

////////            return line[index];
////////        }

////////        private void Consume()
////////        {
////////            position++;
////////            if (mode == LexMode.Default)
////////            {
////////                while (char.IsWhiteSpace(Peek()))
////////                    position++;
////////            }
////////        }

////////        private void ConsumeToEnd() => position = line.Length;

////////        private static bool IsUnsignedIntegerPart(char c) => char.IsNumber(c);
////////        private static bool IsSignedIntegerPart(char c) => IsUnsignedIntegerPart(c) || c == '-' || c == '+';
////////        private static bool IsRealPart(char c) => IsSignedIntegerPart(c) || c == '.';

////////        private static string NormalizeWordOrLineNumberOrStar(StringBuilder builder)
////////        {
////////            var outBuilder = new StringBuilder(builder.Capacity);
////////            for (var index = 0; index < builder.Length; index++)
////////            {
////////                var c = builder[index];
////////                if (char.IsLetter(c))
////////                    outBuilder.Append(char.ToUpperInvariant(c));
////////                else if (!char.IsWhiteSpace(c))
////////                    outBuilder.Append(c);
////////            }
////////            return outBuilder.ToString();
////////        }
////////    }
////////}
