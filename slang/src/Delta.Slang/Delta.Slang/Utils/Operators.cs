using Delta.Slang.Syntax;

namespace Delta.Slang.Utils
{
    // TODO: merge this with semantic UnaryOperator & BinaryOperator?
    internal static class Operators
    {
        public static int GetUnaryOperatorPrecedence(this TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Exclamation:
                //case TokenKind.Tilde:
                    return 60;

                default:
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence(this TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.Star:
                case TokenKind.Slash:
                    return 50;

                case TokenKind.Plus:
                case TokenKind.Minus:
                    return 40;

                case TokenKind.EqualEqual:
                case TokenKind.ExclamationEqual:
                case TokenKind.Lower:
                case TokenKind.LowerEqual:
                case TokenKind.Greater:
                case TokenKind.GreaterEqual:
                    return 30;

                //case TokenKind.Ampersand:
                //case TokenKind.AmpersandAmpersand:
                //    return 20;

                //case TokenKind.Pipe:
                //case TokenKind.PipePipe:
                //case TokenKind.Hat:
                //    return 10;

                default:
                    return 0;
            }
        }
    }
}
