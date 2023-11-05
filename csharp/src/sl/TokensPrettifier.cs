using System;
using System.Collections.Generic;
using Slang.Syntax;

namespace Slang.Cli;

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