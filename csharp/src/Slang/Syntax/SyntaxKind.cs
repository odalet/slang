using System;
using System.Collections.Generic;

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

    ////SingleQuoteToken,           // '
    ////DoubleQuoteToken,           // "
    EqualsToken, // =
    BangToken, // !
    BangEqualToken, // !=
    EqualsEqualsToken, // ==
    GreaterThanToken, // >
    GreaterThanEqualsToken, // >=
    LessThanToken, // <
    LessThanEqualsToken, // <=
    ////AmpersandAmpersandToken,    // &&
    ////PipePipeToken,              // ||

    // Literals
    IdentifierToken,
    StringLiteralToken,
    IntegerLiteralToken,
    FloatLiteralToken,

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

public enum SyntaxCategory
{
    Invalid,
    Eof,
    Operator,
    Literal,
    Reserved,
    Identifier,
    Trivia,
    NonTerminal
}

public static class SyntaxTokenExtensions
{
    public static SyntaxCategory GetCategory(this SyntaxKind kind) => kind switch
    {
        Invalid => SyntaxCategory.Invalid,
        EofToken => SyntaxCategory.Eof,
        OpenParenToken => SyntaxCategory.Operator,
        CloseParenToken => SyntaxCategory.Operator,
        OpenBraceToken => SyntaxCategory.Operator,
        CloseBraceToken => SyntaxCategory.Operator,
        CommaToken => SyntaxCategory.Operator,
        ColonToken => SyntaxCategory.Operator,
        SemicolonToken => SyntaxCategory.Operator,
        DotToken => SyntaxCategory.Operator,
        PlusToken => SyntaxCategory.Operator,
        MinusToken => SyntaxCategory.Operator,
        StarToken => SyntaxCategory.Operator,
        SlashToken => SyntaxCategory.Operator,
        EqualsToken => SyntaxCategory.Operator,
        BangToken => SyntaxCategory.Operator,
        BangEqualToken => SyntaxCategory.Operator,
        EqualsEqualsToken => SyntaxCategory.Operator,
        GreaterThanToken => SyntaxCategory.Operator,
        GreaterThanEqualsToken => SyntaxCategory.Operator,
        LessThanToken => SyntaxCategory.Operator,
        LessThanEqualsToken => SyntaxCategory.Operator,
        IdentifierToken => SyntaxCategory.Identifier,
        StringLiteralToken => SyntaxCategory.Literal,
        IntegerLiteralToken => SyntaxCategory.Literal,
        FloatLiteralToken => SyntaxCategory.Literal,
        GotoToken => SyntaxCategory.Reserved,
        FunToken => SyntaxCategory.Reserved,
        ValToken => SyntaxCategory.Reserved,
        VarToken => SyntaxCategory.Reserved,
        IfToken => SyntaxCategory.Reserved,
        ElseToken => SyntaxCategory.Reserved,
        WhileToken => SyntaxCategory.Reserved,
        BreakToken => SyntaxCategory.Reserved,
        ContinueToken => SyntaxCategory.Reserved,
        ReturnToken => SyntaxCategory.Reserved,
        TrueToken => SyntaxCategory.Reserved,
        FalseToken => SyntaxCategory.Reserved,
        PrintToken => SyntaxCategory.Reserved,
        PrintlnToken => SyntaxCategory.Reserved,
        WhitespaceTrivia => SyntaxCategory.Trivia,
        CommentTrivia => SyntaxCategory.Trivia,
        _ => SyntaxCategory.NonTerminal
    };
}

internal static class ReservedWords
{
    private static readonly Dictionary<string, SyntaxKind> reservedWords = new()
    {
        ["goto"] = GotoToken,
        ["fun"] = FunToken,
        ["val"] = ValToken,
        ["var"] = VarToken,
        ["if"] = IfToken,
        ["else"] = ElseToken,
        ["while"] = WhileToken,
        ["break"] = BreakToken,
        ["continue"] = ContinueToken,
        ["return"] = ReturnToken,
        ["true"] = TrueToken,
        ["false"] = FalseToken,
        ["print"] = PrintToken,
        ["println"] = PrintlnToken,
    };

    public static SyntaxKind? TryGetToken(string text) =>
        reservedWords.TryGetValue(text, out var kind) ? kind : null;
}