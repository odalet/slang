namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;

    partial class Lexer
    {
        private void LexOperatorEndingWithOptionalEqual(char firstCharacter, ref TokenInfo info)
        {
            Consume();
            var hasAdditionalEqual = false;
            if (LookAhead() == '=')
            {
                Consume();
                hasAdditionalEqual = true;
            }

            info.Kind = firstCharacter switch
            {
                '<' => hasAdditionalEqual ? LessEqualToken : LessToken,
                '>' => hasAdditionalEqual ? GreaterEqualToken : GreaterToken,
                '=' => hasAdditionalEqual ? EqualEqualToken : EqualToken,
                '!' => hasAdditionalEqual ? BangEqualToken : BangToken,
                _ => InvalidToken // Theoretically, this code is never reached
            };
        }

        private void LexLogicalOperator(char firstCharacter, ref TokenInfo info)
        {
            Consume();
            var isDuplicated = false;
            if (LookAhead() == firstCharacter)
            {
                Consume();
                isDuplicated = true;
            }

            if (!isDuplicated)
                info.Kind = InvalidToken; // Later on, this will be a bit manipulation operator
            else info.Kind = firstCharacter switch
            {
                '&' => LogicalAndToken,
                '|' => LogicalOrToken,
                _ => InvalidToken // Theoretically, this code is never reached
            };
        }
    }
}
