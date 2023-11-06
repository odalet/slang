using System;
using Slang.Diagnostics;
using Slang.Utils;

namespace Slang.Syntax;

using static CharacterUtils;

internal ref struct ScannerState
{
    private readonly ReadOnlySpan<char> text;
    private readonly int length;
    private (int line, int column) previousLinePosition;
    private (int line, int column) currentLinePosition;
    private int position;
    private TokenInfo info;

    public ScannerState(ReadOnlySpan<char> sourceText)
    {
        text = sourceText;
        length = text.Length;
    }

    public readonly ReadOnlySpan<char> CurrentSpan =>
        text[info.Location.Start..position];

    public void StartNewLexeme()
    {
        previousLinePosition = currentLinePosition; // Start scanning a new lexeme
        info = new TokenInfo(position);
    }

    public readonly char LookAhead() => position >= length ? InvalidCharacter : text[position];
    public readonly char LookAhead(int n) => position + n >= length ? InvalidCharacter : text[position + n];
    public readonly SyntaxToken GetToken() => info.ToToken();

    public void Consume()
    {
        if (position >= length) return;

        position++;
        currentLinePosition.column++;
    }

    public bool ConsumeLineBreakIfAny()
    {
        var ch = LookAhead();
        if (ch == '\r') // Specific case: \r and \r\n
        {
            currentLinePosition.line++;
            Consume();

            var next = LookAhead();
            if (next == '\n')
                Consume();

            currentLinePosition.column = 0;
            return true;
        }

        if (IsNewLine(ch))
        {
            currentLinePosition.line++;
            Consume();

            currentLinePosition.column = 0;
            return true;
        }

        return false;
    }

    public void SetDiagnostic(DiagnosticCode diagnosticCode) => info.SetDiagnostic(diagnosticCode);

    public void UpdateInfo(SyntaxKind kind) => info.Update(
        kind, position, previousLinePosition, currentLinePosition);
}