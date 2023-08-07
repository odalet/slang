namespace Delta.Slang.Syntax;

public enum TokenKind
{
    Eof,
    Invalid,

    Plus,
    Minus,
    Star,
    Slash,
    Percent,
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    Colon,
    Comma,
    Semicolon,
    Lower,
    LowerEqual,
    Greater,
    GreaterEqual,
    Exclamation,
    ExclamationEqual,
    Equal,
    EqualEqual,
    DoubleQuote,

    Whitespace,
    StringLiteral,
    IntLiteral,
    DoubleLiteral,

    Comment,

    GotoKeyword,
    FunKeyword,
    VarKeyword,
    ReturnKeyword,
    TrueKeyword,
    FalseKeyword,
    IfKeyword,
    ElseKeyword,

    Identifier
}

internal static class TokenKindConversions
{
    public static TokenKind GetIdentifierOrKeyword(string text) => text switch
    {
        "goto" => TokenKind.GotoKeyword,
        "fun" => TokenKind.FunKeyword,
        "var" => TokenKind.VarKeyword,
        "if" => TokenKind.IfKeyword,
        "else" => TokenKind.ElseKeyword,
        "return" => TokenKind.ReturnKeyword,
        "true" => TokenKind.TrueKeyword,
        "false" => TokenKind.FalseKeyword,
        _ => TokenKind.Identifier,
    };

    public static string GetText(TokenKind kind) => kind switch
    {
        TokenKind.Plus => "+",
        TokenKind.Minus => "-",
        TokenKind.Star => "*",
        TokenKind.Slash => "/",
        TokenKind.Exclamation => "!",
        TokenKind.ExclamationEqual => "!=",
        TokenKind.Equal => "=",
        TokenKind.EqualEqual => "==",
        //case TokenKind.Tilde: return "~";
        TokenKind.Lower => "<",
        TokenKind.LowerEqual => "<=",
        TokenKind.Greater => ">",
        TokenKind.GreaterEqual => ">=",
        //case TokenKind.Ampersand: return "&";
        //case TokenKind.AmpersandAmpersand: return "&&";
        //case TokenKind.Pipe: return "|";
        //case TokenKind.PipePipe: return "||";
        //case TokenKind.Hat: return "^";
        TokenKind.OpenParenthesis => "(",
        TokenKind.CloseParenthesis => ")",
        TokenKind.OpenBrace => "{",
        TokenKind.CloseBrace => "}",
        TokenKind.Colon => ":",
        TokenKind.Comma => ",",
        TokenKind.Semicolon => ";",
        TokenKind.FalseKeyword => "false",
        TokenKind.TrueKeyword => "true",
        TokenKind.VarKeyword => "var",
        TokenKind.ReturnKeyword => "return",
        //case TokenKind.BreakKeyword: return "break";
        //case TokenKind.ContinueKeyword: return "continue";                
        //case TokenKind.ForKeyword: return "for";
        TokenKind.GotoKeyword => "goto",
        TokenKind.FunKeyword => "fun",
        //case TokenKind.FunctionKeyword: return "function";
        TokenKind.IfKeyword => "if",
        TokenKind.ElseKeyword => "else",
        //case TokenKind.LetKeyword: return "let";
        //case TokenKind.ToKeyword: return "to";
        //case TokenKind.WhileKeyword: return "while";
        //case TokenKind.DoKeyword: return "do";
        _ => "<?>",
    };
}

internal static class TokenKindExtensions
{
    public static string GetText(this TokenKind kind) => TokenKindConversions.GetText(kind);
}
