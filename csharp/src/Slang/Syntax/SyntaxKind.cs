using System;

namespace Slang.Syntax;

using static SyntaxKind;

public enum SyntaxKind : ushort
{
    Invalid = 0,

    // *************** Terminals ***************
    EofToken,

    // Operators
    OpenParenToken, // (
    CloseParenToken, // )
    OpenBraceToken, // {
    CloseBraceToken, // }
    CommaToken, // ,
    ColonToken, // :
    SemicolonToken, // ;
    DotToken, // .
    PlusToken, // +
    MinusToken, // -
    StarToken, // *
    SlashToken, // /

    //SingleQuoteToken,           // '
    //DoubleQuoteToken,           // "
    EqualsToken, // =
    BangToken, // !
    BangEqualToken, // !=
    EqualsEqualsToken, // ==
    GreaterThanToken, // >
    GreaterThanEqualsToken, // >=
    LessThanToken, // <
    LessThanEqualsToken, // <=
    //AmpersandAmpersandToken,    // &&
    //PipePipeToken,              // ||

    // Literals
    IdentifierToken,
    StringLiteralToken,
    NumberLiteralToken,

    // Reserved Words
    GotoToken,
    FunToken,
    ValToken,
    VarToken,
    IfToken,
    ElseToken,
    WhileToken,
    BreakToken,
    ContinueToken,
    ReturnToken,
    TrueToken,
    FalseToken,
    PrintToken, // Temporary: as long as we do not have functions nor a real runtime lib
    PrintlnToken, // Temporary: as long as we do not have functions nor a real runtime lib

    // *************** Trivia ***************
    // Trivia are terminals, but not needed to compile
    WhitespaceTrivia,
    CommentTrivia,

    // *************** Non-Terminals ***************
}

internal static class ReservedWords
{
    private static readonly (string text, SyntaxKind kind)[] reservedWords = new[]
    {
        ("goto", GotoToken),
        ("fun", FunToken),
        ("val", ValToken),
        ("var", VarToken),
        ("if", IfToken),
        ("else", ElseToken),
        ("while", WhileToken),
        ("break", BreakToken),
        ("continue", ContinueToken),
        ("return", ReturnToken),
        ("true", TrueToken),
        ("false", FalseToken),
        ("print", PrintToken),
        ("println", PrintlnToken)
    };

    public static SyntaxKind? TryGetToken(ReadOnlySpan<char> text)
    {
        // See https://stackoverflow.com/questions/49289559/spanchar-and-string-equality
        foreach (var (value, kind) in reservedWords)
        {
            if (MemoryExtensions.Equals(text, value, StringComparison.Ordinal))
                return kind;
        }

        return null;
    }
}