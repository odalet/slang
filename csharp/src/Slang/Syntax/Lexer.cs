using System;
using System.Collections.Generic;
using Slang.Diagnostics;
using Slang.Utils;

namespace Slang.Syntax;

public ref struct Lexer
{
    private Scanner scanner;

    public Lexer(ReadOnlySpan<char> text)
    {
        var state = new ScannerState(text);
        scanner = new Scanner(state);
    }

    public SyntaxToken[] Lex()
    {
        var list = new List<SyntaxToken>(); // NB: we cannot yield return when using a span
        while (true)
        {
            var tok = scanner.Next();
            list.Add(tok);

            if (tok.Kind == SyntaxKind.EofToken)
                break;
        }

        return list.ToArray();
    }
}

internal struct TokenInfo
{
    public TokenInfo(int start) => Location = new(start, 0);

    public SyntaxKind Kind { get; private set; }
    public DiagnosticCode DiagnosticCode { get; private set; }
    public TextLocation Location { get; private set; }
    public LinePosition EndLinePosition { get; private set; }
    public LinePosition StartLinePosition { get; private set; }

    public readonly SyntaxToken ToToken() => new(this);

    public void Update(SyntaxKind kind, int endPosition, (int line, int column) startLinePosition, (int line, int column) endLinePosition)
    {
        Kind = kind;
        Location = Location.WithEnd(endPosition);
        StartLinePosition = new(startLinePosition.line, startLinePosition.column);
        EndLinePosition = new(endLinePosition.line, endLinePosition.column);
    }

    public void SetDiagnostic(DiagnosticCode diagnosticCode) => DiagnosticCode = diagnosticCode;
}