using Delta.Slang.Text;

namespace Delta.Slang.Syntax
{
    public sealed class Token
    {
        public Token(TokenKind kind, TextSpan span, string original) : this(kind, span, original, original) { }
        public Token(TokenKind kind, TextSpan span, string original, string normalized)
        {
            Kind = kind;
            Span = span;
            OriginalText = original ?? string.Empty;
            Text = normalized ?? string.Empty;
        }

        public TokenKind Kind { get; }
        public TextSpan Span { get; }
        public string OriginalText { get; }

        public string Text { get; }

        public static Token MakeEof(int position) => new Token(TokenKind.Eof, new TextSpan(position, 1), "\0");
        public static Token MakeEol(int position) => new Token(TokenKind.Eol, new TextSpan(position, 1), "\0");

        public override string ToString() => $"{Kind} @{Span.Start}: {Text}";        
    }
}
