using System;
using System.Collections.Generic;
using Slang.Diagnostics;
using Slang.Utils;

namespace Slang.Syntax;

using static SyntaxKind;
using static CharacterUtils;

internal struct TokenInfo
{
    private readonly List<DiagnosticCode> diagnosticCodes = new();

    public TokenInfo(int start) => Start = start;

    public SyntaxKind Kind { get; set; }
    public readonly DiagnosticCode[] DiagnosticCodes => diagnosticCodes.ToArray();
    public int Start { get; }
    public int Length { get; set; }
    public (int line, int column) StartLinePosition { get; set; }
    public (int line, int column) EndLinePosition { get; set; }

    // TODO: Have this in the token
    //public bool IsValid => DiagnosticCode == DiagnosticCode.None;

    public readonly SyntaxToken ToToken() => new(
        Kind,
        new(Start, Length),
        new(StartLinePosition.line, StartLinePosition.column),
        new(EndLinePosition.line, EndLinePosition.column));

    public readonly void AddDiagnostic(DiagnosticCode diagnosticCode) =>
        diagnosticCodes.Add(diagnosticCode);
}

// Only scans the next token
internal sealed class Scanner
{
    private const char invalidCharacter = char.MaxValue;
    ////private readonly string sourceText;
    private readonly int maxLength;
    private (int line, int column) previousLinePosition;
    private (int line, int column) currentLinePosition;
    private int position = 0;
    private TokenInfo info;

    public Scanner(int length) => maxLength = length;

    public TokenInfo Next(ReadOnlySpan<char> text)
    {
        Start();
        info = new TokenInfo(position);

        var ch = LookAhead(text);
        switch (ch)
        {
            case '\0' or invalidCharacter: ScanSingleCharacterToken(EofToken); break;
            case '(': ScanSingleCharacterToken(OpenParenToken); break;
            case ')': ScanSingleCharacterToken(CloseParenToken); break;
            case '{': ScanSingleCharacterToken(OpenBraceToken); break;
            case '}': ScanSingleCharacterToken(CloseBraceToken); break;
            case ',': ScanSingleCharacterToken(CommaToken); break;
            case ':': ScanSingleCharacterToken(ColonToken); break;
            case ';': ScanSingleCharacterToken(SemicolonToken); break;
            case '.': ScanSingleCharacterToken(DotToken); break;
            case '+': ScanSingleCharacterToken(PlusToken); break;
            case '-': ScanSingleCharacterToken(MinusToken); break;
            case '*': ScanSingleCharacterToken(StarToken); break;
            case '/': ScanSlash(text); break; //update(SlashToken); break; // TODO: comments
            case '\'': ScanSingleCharacterToken(SingleQuoteToken); break;
            case '\"': ScanSingleCharacterToken(DoubleQuoteToken); break;
            case '=': ScanSingleCharacterToken(EqualsToken); break;
            case '!': ScanSingleCharacterToken(BangToken); break;
            case '>': ScanSingleCharacterToken(GreaterThanToken); break;
            case '<': ScanSingleCharacterToken(LessThanToken); break;
            default:
                if (IsWhitespaceOrNewLine(ch))
                    ScanWhitespace(text);
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
        info.AddDiagnostic(DiagnosticCode.ErrorInvalidToken);
        UpdateInfo(None);
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

    private void ScanSlash(ReadOnlySpan<char> text)
    {
        Consume(); // This consumes the initial /
        info.Kind = LookAhead(text) switch
        {
            '/' => ScanCppComment(text),
            '*' => ScanCComment(text),
            _ => SlashToken,
        };

        UpdateInfo();
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
                info.AddDiagnostic(DiagnosticCode.ErrorUnterminatedComment);
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
                    info.AddDiagnostic(DiagnosticCode.ErrorUnterminatedComment);
                    break;
                }
            }

            Consume(); // All other cases: Keep consuming
        }

        return CommentTrivia;
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

    // Start scanning a new lexeme
    private void Start()
    {
        previousLinePosition = currentLinePosition;
    }

    private void UpdateInfo(SyntaxKind kind)
    {
        info.Kind = kind;
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        info.Length = position - info.Start;
        info.StartLinePosition = previousLinePosition;
        info.EndLinePosition = currentLinePosition;
    }

    private void ScanToEndOfLine(ReadOnlySpan<char> text)
    {
        while (
            !IsNewLine(text[position]) &&
            text[position] != invalidCharacter &&
            position < text.Length)
            Consume();
    }
    private static bool IsWhitespaceOrNewLine(char c) => IsWhitespace(c) || IsNewLine(c);

    ////private void AddDiagnostic(DiagnosticCode code)
    ////{
    ////    //var diagnostic = new SyntaxDiagnostic(code, )
    ////}


    ////////// new-line-character:
    //////////   Carriage return character (U+000D)
    //////////   Line feed character (U+000A)
    //////////   Next line character (U+0085)
    //////////   Line separator character (U+2028)
    //////////   Paragraph separator character (U+2029)
    ////////private static bool IsNewLine(in char ch) => ch is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029';

    ////////// whitespace:
    //////////   Any character with Unicode class Zs
    //////////   Horizontal tab character (U+0009)
    //////////   Vertical tab character (U+000B)
    //////////   Form feed character (U+000C)
    ////////private static bool IsWhitespace(in char ch)

    ////private void InitializeAndConsume(ref TokenInfo info, SyntaxKind kind, int start, int length)
    ////{
    ////    info.Kind = kind;
    ////    info.Start = start;
    ////    info.Length = length;
    ////    Consume();
    ////}
}

public sealed class Lexer
{
    private readonly string sourceText;
    private readonly Scanner scanner;

    public Lexer(string text)
    {
        sourceText = text;
        scanner = new Scanner(text.Length);
    }

    public IEnumerable<SyntaxToken> Lex()
    {
        var text = sourceText.AsSpan();
        var list = new List<SyntaxToken>(); // NB: we cannot yield return when using a span
        while (true)
        {
            var info = scanner.Next(text);
            list.Add(info.ToToken());

            if (info.Kind == SyntaxKind.EofToken)
                break;
        }

        return list;
    }
}
