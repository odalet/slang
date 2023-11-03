using System;
using System.Linq;
using Slang.Diagnostics;
using Slang.Utils;

namespace Slang.Syntax;

public readonly struct SyntaxToken
{
    private readonly TokenInfo info;

    internal SyntaxToken(TokenInfo tokenInfo) => info = tokenInfo;

    public SyntaxKind Kind => info.Kind;
    public TextLocation Location => info.Location;
    public LinePosition StartLinePosition => info.StartLinePosition;
    public LinePosition EndLinePosition => info.EndLinePosition;
    public DiagnosticCode[] DiagnosticCodes => info.DiagnosticCodes;

    public bool IsValid => DiagnosticCodes.Length == 0 || Array.TrueForAll(
        DiagnosticCodes, c => c == DiagnosticCode.None);
}