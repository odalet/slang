namespace Slang.Syntax;

public enum SyntaxKind : ushort
{
    None = 0,

    // *************** Terminals ***************
    FooToken,
    EofToken,

    // Operators
    OpenParenToken,             // (
    CloseParenToken,            // )
    OpenBraceToken,             // {
    CloseBraceToken,            // }
    CommaToken,                 // ,
    ColonToken,                 // :
    SemicolonToken,             // ;
    DotToken,                   // .
    PlusToken,                  // +
    MinusToken,                 // -
    StarToken,                  // *
    SlashToken,                 // /
    SingleQuoteToken,           // '
    DoubleQuoteToken,           // "
    EqualsToken,                // =
    BangToken,                  // !
    BangEqualToken,             // !=
    EqualsEqualsToken,          // ==
    GreaterThanToken,           // >
    GreaterThanEqualsToken,     // >=
    LessThanToken,              // <
    LessThanEqualsToken,        // <=
    AmpersandAmpersandToken,    // &&
    PipePipeToken,              // ||

    // *************** Trivia ***************
    // Trivia are terminals, but not needed to compile
    WhitespaceTrivia,
    CommentTrivia,

    // *************** Non-Terminals ***************
}
