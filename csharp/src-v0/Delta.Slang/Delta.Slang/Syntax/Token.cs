using Delta.Slang.Text;

namespace Delta.Slang.Syntax;

public sealed class Token
{
    public Token(TokenKind kind, TextSpan span, LinePosition position, string text, bool isForged = false) : this(kind, span, position, text, text, isForged) { }
    public Token(TokenKind kind, TextSpan span, LinePosition position, string text, object value, bool isForged = false)
    {
        Kind = kind;
        Span = span;
        Position = position;
        Text = text ?? string.Empty;
        Value = value;
        IsForged = isForged;
    }

    public TokenKind Kind { get; }
    public TextSpan Span { get; }
    public LinePosition Position { get; }
    public string Text { get; }
    public object Value { get; } // Contains the parsed int if Kind is number
    public bool IsForged { get; } // true if the token was created by the parser

    public static Token MakeEof(int position, LinePosition line) => new(TokenKind.Eof, new TextSpan(position, 1), line, "\0");

    public override string ToString() => $"{Kind} @{Span.Start}: {Text}";
}

public static class TokenExtensions
{
    public static bool IsKeyword(this Token token) => token.Kind.ToString().EndsWith("Keyword");
}
