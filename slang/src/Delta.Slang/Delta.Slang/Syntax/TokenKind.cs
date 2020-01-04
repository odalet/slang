namespace Delta.Slang.Syntax
{
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
        NumberLiteral,

        Comment,

        FunKeyword,
        VarKeyword,
        ReturnKeyword,
        TrueKeyword,
        FalseKeyword,

        Identifier
    }

    internal static class TokenKindExtensions
    {
        public static string GetText(this TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.Plus: return "+";
                case TokenKind.Minus: return "-";
                case TokenKind.Star: return "*";
                case TokenKind.Slash: return "/";
                case TokenKind.Exclamation: return "!";
                case TokenKind.ExclamationEqual: return "!=";
                case TokenKind.Equal: return "=";
                case TokenKind.EqualEqual: return "==";
                //case TokenKind.Tilde: return "~";
                case TokenKind.Lower: return "<";
                case TokenKind.LowerEqual: return "<=";
                case TokenKind.Greater: return ">";
                case TokenKind.GreaterEqual: return ">=";
                //case TokenKind.Ampersand: return "&";
                //case TokenKind.AmpersandAmpersand: return "&&";
                //case TokenKind.Pipe: return "|";
                //case TokenKind.PipePipe: return "||";
                //case TokenKind.Hat: return "^";
                case TokenKind.OpenParenthesis: return "(";
                case TokenKind.CloseParenthesis: return ")";
                case TokenKind.OpenBrace: return "{";
                case TokenKind.CloseBrace: return "}";
                case TokenKind.Colon: return ":";
                case TokenKind.Comma: return ",";
                case TokenKind.Semicolon: return ";";
                case TokenKind.FalseKeyword: return "false";
                case TokenKind.TrueKeyword: return "true";
                case TokenKind.VarKeyword: return "var";
                case TokenKind.ReturnKeyword: return "return";
                //case TokenKind.BreakKeyword: return "break";
                //case TokenKind.ContinueKeyword: return "continue";
                //case TokenKind.ElseKeyword: return "else";
                //case TokenKind.ForKeyword: return "for";
                case TokenKind.FunKeyword: return "fun";
                //case TokenKind.FunctionKeyword: return "function";
                //case TokenKind.IfKeyword: return "if";
                //case TokenKind.LetKeyword: return "let";
                //case TokenKind.ToKeyword: return "to";
                //case TokenKind.WhileKeyword: return "while";
                //case TokenKind.DoKeyword: return "do";
                default:
                    return "<?>";
            }
        }
    }
}
