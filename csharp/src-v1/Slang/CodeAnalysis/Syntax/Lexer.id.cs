using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Slang.CodeAnalysis.Syntax;

using static SyntaxKind;

partial class Lexer
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

        // Special cases: boolean literals
        if (info.Kind == TrueToken) info.Value = true;
        if (info.Kind == FalseToken) info.Value = false;
    }
}
