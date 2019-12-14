namespace Delta.Slang.Syntax
{
    public enum TokenKind
    {
        Eof,
        Eol,
        Invalid,

        // Lexemes
        OpenParenthesis,
        CloseParenthesis,
        Semicolon,
        DoubleQuote,
        Whitespace,
        Identifier,
        StringLiteral,

        ////////// GCode
        ////////LineNumber,
        ////////Word,
        ////////Star, // Reprap-specific: contains a check-sum
        Comment,        
    }
}
