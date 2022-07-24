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
        public string SanitizedText => Text
            .Replace("\t", "\\t")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\0", "\\0")
            ;

        // Beware: Eof is not a trivial token: it is used in the parser as a stop condition!
        public static Token MakeEof(int position, LinePosition line) => new(EofToken, Terminal, new TextSpan(position, 1), line, "\0");
        public override string ToString() => $"{Kind} @{Span.Start}: '{SanitizedText}'";
    }
}
