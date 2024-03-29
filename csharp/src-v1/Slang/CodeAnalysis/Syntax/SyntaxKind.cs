﻿namespace Slang.CodeAnalysis.Syntax;

// Lists both tokens emitted by the lexer and non-terminal nodes generated by the parser
public enum SyntaxKind : ushort
{
    // -- Terminals --
    EofToken,
    InvalidToken,
    // Operators
    LeftParenToken,     // (
    RightParenToken,    // )
    LeftBraceToken,     // {
    RightBraceToken,    // }
    CommaToken,         // ,
    ColonToken,         // :
    SemicolonToken,     // ;
    DotToken,           // .
    PlusToken,          // +
    MinusToken,         // -
    StarToken,          // *
    SlashToken,         // /
    BangToken,          // !
    BangEqualToken,     // !=
    EqualToken,         // =
    EqualEqualToken,    // ==
    GreaterToken,       // >
    GreaterEqualToken,  // >=
    LessToken,          // <
    LessEqualToken,     // <=
    LogicalAndToken,    // &&
    LogicalOrToken,     // ||

    // Literals
    IdentifierToken,
    StringLiteralToken,
    IntegerLiteralToken,
    FloatLiteralToken,

    // Trivia
    CommentToken,
    DoubleQuoteToken,
    WhitespaceToken,

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
    PrintToken,     // Temporary: as long as we do not have functions nor a real runtime lib
    PrintlnToken,   // Temporary: as long as we do not have functions nor a real runtime lib

    // -- Non-terminals --

    Invalid,
    CompilationUnit,

    // Statements
    EmptyStatement,
    BlockStatement,
    PrintStatement,
    IfStatement,
    WhileStatement,
    BreakStatement,
    ContinueStatement,
    VariableDeclaration,

    // Expressions
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    GroupingExpression,
    VariableExpression,
    LiteralExpression
}

public static class SyntaxKindExtensions
{
    public static bool IsLogicalOperator(this SyntaxKind kind) => kind is SyntaxKind.LogicalAndToken or SyntaxKind.LogicalOrToken;
}
