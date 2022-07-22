using Slang.CodeAnalysis.Text;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;
    using static TokenCategory;

    public enum TokenCategory
    {
        Trivia,
        Terminal,
        NonTerminal
    }

    public sealed record Token(SyntaxKind Kind, TokenCategory Category, TextSpan Span, LinePosition Position, string Text, object? Value = null)
    {
        public static Token MakeEof(int position, LinePosition line) => new(EofToken, Trivia, new TextSpan(position, 1), line, "\0");
        public override string ToString() => $"{Kind} @{Span.Start}: {Text}";
    }
}
