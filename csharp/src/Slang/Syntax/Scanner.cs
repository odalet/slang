using System.Runtime.CompilerServices;
using Slang.Diagnostics;
using Slang.Utils;

namespace Slang.Syntax;

using static CharacterUtils;
using static SyntaxKind;

internal ref struct Scanner
{
    private ScannerState state;

    public Scanner(ScannerState scannerState) => state = scannerState;

    public SyntaxToken Next()
    {
        state.StartNewLexeme();
        Scan();
        return state.GetToken();
    }

    private void Scan()
    {
        var ch = state.LookAhead();
        switch (ch)
        {
            case '\0' or InvalidCharacter:
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
                ScanSlash();
                break;
            case '\"':
                ScanStringLiteral();
                break;
            case '=':
                ScanEquals();
                break;
            case '!':
                ScanBang();
                break;
            case '>':
                ScanGreaterThan();
                break;
            case '<':
                ScanLessThan();
                break;
            default:
                if (IsWhitespaceOrNewLine(ch))
                    ScanWhitespace();
                else if (IsDecimalDigit(ch))
                    ScanNumberLiteral(ch);
                else if (IsIdentifierFirstCharacter(ch))
                    ScanIdentifierOrReservedWord();
                else
                    ScanInvalidToken();
                break;
        }
    }

    private void ScanSingleCharacterToken(SyntaxKind kind)
    {
        state.Consume(); // Consume the character
        state.UpdateInfo(kind);
    }

    private void ScanInvalidToken()
    {
        state.Consume(); // Consume the character
        state.SetDiagnostic(DiagnosticCode.ErrorInvalidToken);
        state.UpdateInfo(Invalid);
    }

    private void ScanWhitespace()
    {
        while (true)
        {
            if (state.ConsumeLineBreakIfAny())
                continue;

            var ch = state.LookAhead();
            if (!IsWhitespace(ch))
                break;

            state.Consume(); // Consume whitespaces
        }

        state.UpdateInfo(WhitespaceTrivia);
    }

    private void ScanEquals()
    {
        state.Consume(); // This consumes the first =
        if (state.LookAhead() == '=')
        {
            state.Consume(); // This consumes the second =
            state.UpdateInfo(EqualsEqualsToken);
        }
        else state.UpdateInfo(EqualsToken);
    }

    private void ScanBang()
    {
        state.Consume(); // This consumes the !
        if (state.LookAhead() == '=')
        {
            state.Consume(); // This consumes the =
            state.UpdateInfo(BangEqualToken);
        }
        else state.UpdateInfo(BangToken);
    }

    private void ScanGreaterThan()
    {
        state.Consume(); // This consumes the first >
        if (state.LookAhead() == '=')
        {
            state.Consume(); // This consumes the second =
            state.UpdateInfo(GreaterThanEqualsToken);
        }
        else state.UpdateInfo(GreaterThanToken);
    }

    private void ScanLessThan()
    {
        state.Consume(); // This consumes the first <
        if (state.LookAhead() == '=')
        {
            state.Consume(); // This consumes the second =
            state.UpdateInfo(LessThanEqualsToken);
        }
        else state.UpdateInfo(LessThanToken);
    }

    private void ScanSlash()
    {
        state.Consume(); // This consumes the initial /
        var kind = state.LookAhead() switch
        {
            '/' => ScanCppComment(),
            '*' => ScanCComment(),
            _ => SlashToken,
        };

        state.UpdateInfo(kind);
    }

    // Comments --------------------------------------------------

    private SyntaxKind ScanCppComment()
    {
        state.Consume(); // This consumes the second /
        while (true)
        {
            var ch = state.LookAhead();
            if (IsNewLine(ch) || ch == InvalidCharacter || ch == '\0')
                break;
            state.Consume(); // Keep consuming until we reach the end of the line or stream
        }

        return CommentTrivia;
    }

    private SyntaxKind ScanCComment()
    {
        state.Consume(); // This consumes the * after the /
        while (true)
        {
            var ch = state.LookAhead();
            if (ch is InvalidCharacter or '\0')
            {
                state.SetDiagnostic(DiagnosticCode.ErrorUnterminatedComment);
                break;
            }

            if (state.ConsumeLineBreakIfAny())
                continue;

            // No support (yet?) for nested comments: we stop at the first */
            if (ch == '*')
            {
                state.Consume();
                var next = state.LookAhead();
                if (next == '/')
                {
                    state.Consume();
                    break;
                }
                else if (next is '\0' or InvalidCharacter)
                {
                    state.SetDiagnostic(DiagnosticCode.ErrorUnterminatedComment);
                    break;
                }
            }

            state.Consume(); // All other cases: Keep consuming
        }

        return CommentTrivia;
    }

    // String literals --------------------------------------------------

    private void ScanStringLiteral()
    {
        state.Consume(); // This consumes the initial quote (")

        var done = false;
        var isEscaping = false;
        while (!done)
        {
            var ch = state.LookAhead();
            switch (ch)
            {
                case '\0' or '\r' or '\n' or InvalidCharacter: // NB: no newline in string literal (for now)
                    state.SetDiagnostic(DiagnosticCode.ErrorUnterminatedStringLiteral);
                    done = true;
                    break;
                case '\\':
                    state.Consume();
                    if (!isEscaping) isEscaping = true;
                    break;
                case '\"':
                    state.Consume();
                    if (!isEscaping) done = true;
                    break;
                default:
                    state.Consume();
                    isEscaping = false; // Not escaping any more...
                    break;
            }
        }

        state.UpdateInfo(StringLiteralToken);
    }

    // Number literals --------------------------------------------------------------------

    // NB: we allow _ anywhere after the eventual base specifier
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDigitOrUnderscore(char c, int numberBase) => c == '_' || numberBase switch
    {
        2 => IsBinaryDigit(c),
        8 => IsOctalDigit(c),
        10 => IsDecimalDigit(c),
        16 => IsHexadecimalDigit(c),
        _ => false
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ConsumeDigits(int numberBase = 10)
    {
        while (IsDigitOrUnderscore(state.LookAhead(), numberBase))
            state.Consume();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ConsumeExponent()
    {
        if (state.LookAhead() is not 'e' and not 'E')
            return false;

        state.Consume(); // This consumes the 'e'

        // We might have a sign
        if (state.LookAhead() is '+' or '-') 
            state.Consume();

        // Consumes the subsequent digits
        ConsumeDigits();

        return true;
    }

    private void ScanNumberLiteral(char initialCharacter)
    {
        state.Consume();

        var numberBase = 10;

        if (initialCharacter == '0')
        {
            numberBase = state.LookAhead() switch
            {
                'b' or 'B' => 2,
                'o' or 'O' => 8,
                'x' or 'X' => 16,
                _ => 10
            };

            if (numberBase != 10)
                state.Consume();
        }

        ConsumeDigits(numberBase);

        var exponentWasFound = ConsumeExponent();
        if (exponentWasFound) // This is the end of the number
        {
            state.UpdateInfo(NumberLiteralToken);
            return;
        }

        // A digit after a dot means we are looking at a decimal separator
        // NB: only supported if the number is decimal
        if (numberBase == 10 && state.LookAhead() == '.' && IsDecimalDigit(state.LookAhead(1)))
        {
            state.Consume(); // This consumes the '.' character
            ConsumeDigits();

            _ = ConsumeExponent(); // Eventual exponent part
        }

        state.UpdateInfo(NumberLiteralToken);
    }

    // Identifiers and Reserved words --------------------------------------------------

    private void ScanIdentifierOrReservedWord()
    {
        while (IsIdentifierCharacter(state.LookAhead()))
            state.Consume();

        // Is it a keyword or an identifier?
        var kind = ReservedWords.TryGetToken(state.CurrentSpan);
        state.UpdateInfo(kind ?? IdentifierToken);
    }
}
