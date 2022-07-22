using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;

    partial class Lexer
    {
        private static readonly Dictionary<string, SyntaxKind> reservedWords = new()
        {
            ["goto"] = GotoToken,
            ["fun"] = FunToken,
            ["var"] = VarToken,
            ["if"] = IfToken,
            ["else"] = ElseToken,
            ["return"] = ReturnToken,
            ["true"] = TrueToken,
            ["false"] = FalseToken,
        };

        // Useful for unit tests
        [ExcludeFromCodeCoverage]
        public static IEnumerable<string> ReservedWords => reservedWords.Keys;

        // Useful for unit tests
        [ExcludeFromCodeCoverage]
        public static bool IsReservedWordToken(SyntaxKind syntaxKind) => reservedWords.ContainsValue(syntaxKind);

        private void LexIdentifierOrReservedWord(ref TokenInfo info)
        {
            while (IsIdentifierCharacter(LookAhead()))
                Consume();

            var text = source.ToString(GetCurrentSpan());
            info.Kind = reservedWords.ContainsKey(text) ? reservedWords[text] : IdentifierToken;
        }
    }
}
