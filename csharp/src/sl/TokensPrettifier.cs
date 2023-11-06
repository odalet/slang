using System;
using System.Collections.Generic;
using Slang.Syntax;

namespace Slang.Cli;

using static SyntaxKind;

internal enum SyntaxCategory
{
    Invalid,
    Eof,
    Operator,
    Literal,
    Reserved,
    Identifier,
    Trivia,
    NonTerminal
}

internal static class SyntaxTokenExtensions
{
    public static SyntaxCategory GetCategory(this SyntaxKind kind) => kind switch
    {
        Invalid => SyntaxCategory.Invalid,
        EofToken => SyntaxCategory.Eof,
        OpenParenToken => SyntaxCategory.Operator,
        CloseParenToken => SyntaxCategory.Operator,
        OpenBraceToken => SyntaxCategory.Operator,
        CloseBraceToken => SyntaxCategory.Operator,
        CommaToken => SyntaxCategory.Operator,
        ColonToken => SyntaxCategory.Operator,
        SemicolonToken => SyntaxCategory.Operator,
        DotToken => SyntaxCategory.Operator,
        PlusToken => SyntaxCategory.Operator,
        MinusToken => SyntaxCategory.Operator,
        StarToken => SyntaxCategory.Operator,
        SlashToken => SyntaxCategory.Operator,
        EqualsToken => SyntaxCategory.Operator,
        BangToken => SyntaxCategory.Operator,
        BangEqualToken => SyntaxCategory.Operator,
        EqualsEqualsToken => SyntaxCategory.Operator,
        GreaterThanToken => SyntaxCategory.Operator,
        GreaterThanEqualsToken => SyntaxCategory.Operator,
        LessThanToken => SyntaxCategory.Operator,
        LessThanEqualsToken => SyntaxCategory.Operator,
        IdentifierToken => SyntaxCategory.Identifier,
        StringLiteralToken => SyntaxCategory.Literal,
        NumberLiteralToken => SyntaxCategory.Literal,
        GotoToken => SyntaxCategory.Reserved,
        FunToken => SyntaxCategory.Reserved,
        ValToken => SyntaxCategory.Reserved,
        VarToken => SyntaxCategory.Reserved,
        IfToken => SyntaxCategory.Reserved,
        ElseToken => SyntaxCategory.Reserved,
        WhileToken => SyntaxCategory.Reserved,
        BreakToken => SyntaxCategory.Reserved,
        ContinueToken => SyntaxCategory.Reserved,
        ReturnToken => SyntaxCategory.Reserved,
        TrueToken => SyntaxCategory.Reserved,
        FalseToken => SyntaxCategory.Reserved,
        PrintToken => SyntaxCategory.Reserved,
        PrintlnToken => SyntaxCategory.Reserved,
        WhitespaceTrivia => SyntaxCategory.Trivia,
        CommentTrivia => SyntaxCategory.Trivia,
        _ => SyntaxCategory.NonTerminal
    };
}

public readonly ref struct TokensPrettifier
{
    private readonly ReadOnlySpan<char> sourceText;

    public TokensPrettifier(ReadOnlySpan<char> text) => sourceText = text;

    public void Dump(IEnumerable<SyntaxToken> tokens)
    {
        foreach (var token in tokens)
        {
            var text = token.GetText(sourceText);

            var previousForeground = Console.ForegroundColor;
            var previousBackground = Console.BackgroundColor;

            if (!token.IsValid)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkRed;
            }
            else
            {
                Console.ForegroundColor = GetColor(token.Kind);
            }

            Console.Write(text);
            Console.BackgroundColor = previousBackground;
            Console.ForegroundColor = previousForeground;

            if (!token.IsValid)
                Console.Write($" /* Error: {token.DiagnosticCode} */ ");
        }

        Console.WriteLine();
    }

    private static ConsoleColor GetColor(SyntaxKind kind) => GetColor(kind.GetCategory());
    private static ConsoleColor GetColor(SyntaxCategory category) => category switch
    {
        SyntaxCategory.Invalid => ConsoleColor.Red,
        SyntaxCategory.Eof => ConsoleColor.Red,
        SyntaxCategory.Operator => ConsoleColor.Green,
        SyntaxCategory.Literal => ConsoleColor.Yellow,
        SyntaxCategory.Reserved => ConsoleColor.Blue,
        SyntaxCategory.Identifier => ConsoleColor.Cyan,
        SyntaxCategory.Trivia => ConsoleColor.Gray,
        SyntaxCategory.NonTerminal => ConsoleColor.Red,
        _ => ConsoleColor.Red
    };
}