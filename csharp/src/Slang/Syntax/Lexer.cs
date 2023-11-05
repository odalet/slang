using System;
using System.Collections.Generic;
using Slang.Diagnostics;
using Slang.Utils;

// ReSharper disable InvertIf

namespace Slang.Syntax;

using static SyntaxKind;
using static CharacterUtils;

public readonly ref struct Lexer
{
    private readonly ReadOnlySpan<char> sourceText;
    private readonly Scanner scanner;

    public Lexer(ReadOnlySpan<char> text)
    {
        sourceText = text;
        scanner = new Scanner(text.Length);
    }

    public SyntaxToken[] Lex()
    {
        var list = new List<SyntaxToken>(); // NB: we cannot yield return when using a span
        while (true)
        {
            var info = scanner.Next(sourceText);
            list.Add(info.ToToken());

            if (info.Kind == SyntaxKind.EofToken)
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

// Only scans the next token
internal sealed class Scanner
{
    private const char invalidCharacter = char.MaxValue;

    private readonly int maxLength;
    private (int line, int column) previousLinePosition;
    private (int line, int column) currentLinePosition;
    private int position;
    private TokenInfo info;

    public Scanner(int length) => maxLength = length;

    public TokenInfo Next(ReadOnlySpan<char> text)
    {
        previousLinePosition = currentLinePosition; // Start scanning a new lexeme
        info = new TokenInfo(position);

        var ch = LookAhead(text);
        switch (ch)
        {
            case '\0' or invalidCharacter:
                ScanSingleCharacterToken(EofToken);
                break;
            case '(':
                ScanSingleCharacterToken(OpenParenToken);
                break;
            case ')':
                ScanSingleCharacterToken(CloseParenToken);
                break;
            case '{':
                ScanSingleCharacterToken(OpenBraceToken);
                break;
            case '}':
                ScanSingleCharacterToken(CloseBraceToken);
                break;
            case ',':
                ScanSingleCharacterToken(CommaToken);
                break;
            case ':':
                ScanSingleCharacterToken(ColonToken);
                break;
            case ';':
                ScanSingleCharacterToken(SemicolonToken);
                break;
            case '.':
                ScanSingleCharacterToken(DotToken);
                break;
            case '+':
                ScanSingleCharacterToken(PlusToken);
                break;
            case '-':
                ScanSingleCharacterToken(MinusToken);
                break;
            case '*':
                ScanSingleCharacterToken(StarToken);
                break;
            case '/':
                ScanSlash(text);
                break;
            case '\"':
                ScanStringLiteral(text);
                break;
            case '=':
                ScanEquals(text);
                break;
            case '!':
                ScanBang(text);
                break;
            case '>':
                ScanGreaterThan(text);
                break;
            case '<':
                ScanLessThan(text);
                break;
            default:
                if (IsWhitespaceOrNewLine(ch))
                    ScanWhitespace(text);
                else if (IsDigit(ch))
                    ScanNumberLiteral(ch, text);
                else if (IsIdentifierFirstCharacter(ch))
                    ScanIdentifierOrReservedWord(text);
                else
                    ScanInvalidToken();
                break;
        }

        return info;
    }

    private void ScanSingleCharacterToken(SyntaxKind kind)
    {
        Consume(); // Consume the character
        UpdateInfo(kind);
    }

    private void ScanInvalidToken()
    {
        Consume(); // Consume the character
        info.SetDiagnostic(DiagnosticCode.ErrorInvalidToken);
        UpdateInfo(Invalid);
    }

    private void ScanWhitespace(ReadOnlySpan<char> text)
    {
        while (true)
        {
            if (ConsumeLineBreakIfAny(text))
                continue;

            var ch = LookAhead(text);
            if (!IsWhitespace(ch))
                break;

            Consume(); // Consume whitespaces
        }

        UpdateInfo(WhitespaceTrivia);
    }

    private void ScanEquals(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the first =
        if (LookAhead(text) == '=')
        {
            Consume(); // This consumes the second =
            UpdateInfo(EqualsEqualsToken);
        }
        else UpdateInfo(EqualsToken);
    }

    private void ScanBang(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the !
        if (LookAhead(text) == '=')
        {
            Consume(); // This consumes the =
            UpdateInfo(BangEqualToken);
        }
        else UpdateInfo(BangToken);
    }

    private void ScanGreaterThan(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the first >
        if (LookAhead(text) == '=')
        {
            Consume(); // This consumes the second =
            UpdateInfo(GreaterThanEqualsToken);
        }
        else UpdateInfo(GreaterThanToken);
    }

    private void ScanLessThan(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the first <
        if (LookAhead(text) == '=')
        {
            Consume(); // This consumes the second =
            UpdateInfo(LessThanEqualsToken);
        }
        else UpdateInfo(LessThanToken);
    }

    private void ScanSlash(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the initial /
        var kind = LookAhead(text) switch
        {
            '/' => ScanCppComment(text),
            '*' => ScanCComment(text),
            _ => SlashToken,
        };

        UpdateInfo(kind);
    }

    private SyntaxKind ScanCppComment(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the second /
        while (true)
        {
            var ch = LookAhead(text);
            if (IsNewLine(ch) || ch == invalidCharacter || ch == '\0')
                break;
            Consume(); // Keep consuming until we reach the end of the line or stream
        }

        return CommentTrivia;
    }

    private SyntaxKind ScanCComment(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the * after the /
        while (true)
        {
            var ch = LookAhead(text);
            if (ch is invalidCharacter or '\0')
            {
                info.SetDiagnostic(DiagnosticCode.ErrorUnterminatedComment);
                break;
            }

            if (ConsumeLineBreakIfAny(text))
                continue;

            // No support (yet?) for nested comments: we stop at the first */
            if (ch == '*')
            {
                Consume();
                var next = LookAhead(text);
                if (next == '/')
                {
                    Consume();
                    break;
                }
                else if (next is '\0' or invalidCharacter)
                {
                    info.SetDiagnostic(DiagnosticCode.ErrorUnterminatedComment);
                    break;
                }
            }

            Consume(); // All other cases: Keep consuming
        }

        return CommentTrivia;
    }

    private void ScanStringLiteral(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the initial quote (")

        var done = false;
        var isEscaping = false;
        while (!done)
        {
            var ch = LookAhead(text);
            switch (ch)
            {
                case '\0' or '\r' or '\n' or invalidCharacter: // NB: no newline in string literal (for now)
                    info.SetDiagnostic(DiagnosticCode.ErrorUnterminatedStringLiteral);
                    done = true;
                    break;
                case '\\':
                    Consume();
                    if (!isEscaping) isEscaping = true;
                    break;
                case '\"':
                    Consume();
                    if (!isEscaping) done = true;
                    break;
                default:
                    Consume();
                    isEscaping = false; // Not escaping any more...
                    break;
            }
        }

        UpdateInfo(StringLiteralToken);
    }
    
    private void ScanNumberLiteral(char initialCharacter, ReadOnlySpan<char> text)
    {
        Consume();

        var hexMode = initialCharacter == '0' && LookAhead(text) is 'x' or 'X';
        var binMode = initialCharacter == '0' && LookAhead(text) is 'b' or 'B';
        if (hexMode || binMode) Consume();
        
        // NB: we allow _ anywhere after the eventual base specifier
        bool isDigitOrUnderscore(char c, bool decimalOnlyMode = false)
        {
            if (c == '_') return true;
            
            if (decimalOnlyMode) return IsDigit(c);
            
            if (binMode) return c is '0' or '1';
            if (IsDigit(c)) return true;
            if (hexMode) return c 
                is 'a' or 'b' or 'c' or 'd' or 'e' or 'f'
                or 'A' or 'B' or 'C' or 'D' or 'E' or 'F';
            return false;
        }
        
        while (isDigitOrUnderscore(LookAhead(text)))
            Consume();
        
        // TODO: exponent
        
        // A digit after a dot means we are looking at a decimal separator
        // NB: only supported if the number is decimal
        if (!hexMode && !binMode && LookAhead(text) == '.' && IsDigit(LookAhead(text, 1)))
        {
            Consume(); // This consumes the '.' character
            
            while (isDigitOrUnderscore(LookAhead(text), decimalOnlyMode: true))
                Consume();
            
            UpdateInfo(FloatLiteralToken);
            return;
        }

        // Otherwise, don't consume the dot (it will be consumed by the
        // general lexing loop) and build an integer
        UpdateInfo(IntegerLiteralToken);
    }

    private void ScanIdentifierOrReservedWord(ReadOnlySpan<char> text)
    {
        while (IsIdentifierCharacter(LookAhead(text)))
            Consume();

        // Is it a keyword or an identifier?
        var value = text[info.Location.Start..position].ToString();
        var kind = ReservedWords.TryGetToken(value);
        UpdateInfo(kind ?? IdentifierToken);
    }

    private char LookAhead(ReadOnlySpan<char> text) => position >= maxLength ? invalidCharacter : text[position];
    private char LookAhead(ReadOnlySpan<char> text, int n) => position + n >= maxLength ? invalidCharacter : text[position + n];

    private void Consume()
    {
        if (position >= maxLength) return;

        position++;
        currentLinePosition.column++;
    }

    private bool ConsumeLineBreakIfAny(ReadOnlySpan<char> text)
    {
        var ch = LookAhead(text);
        if (ch == '\r') // Specific case: \r and \r\n
        {
            currentLinePosition.line++;
            Consume();

            var next = LookAhead(text);
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
    
    private void UpdateInfo(SyntaxKind kind) => info.Update(
        kind, position, previousLinePosition, currentLinePosition);

    private static bool IsWhitespaceOrNewLine(char c) => IsWhitespace(c) || IsNewLine(c);
    private static bool IsIdentifierFirstCharacter(char c) => c == '_' || char.IsLetter(c);
    private static bool IsIdentifierCharacter(char c) => IsIdentifierFirstCharacter(c) || char.IsDigit(c);
}