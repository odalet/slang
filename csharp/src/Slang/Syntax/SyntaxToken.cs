using System;
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
    public DiagnosticCode DiagnosticCode => info.DiagnosticCode;
    public bool IsValid => DiagnosticCode == DiagnosticCode.None;

    public string GetText(ReadOnlySpan<char> source) => GetSpan(source).ToString();
    private ReadOnlySpan<char> GetSpan(ReadOnlySpan<char> source) => source[Location.Start..Location.End];
}