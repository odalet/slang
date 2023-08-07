using System;
using System.Collections.Generic;
using System.IO;
using Delta.Slang.Syntax;

namespace Delta.Slang.Utils;

public sealed class Highlighter
{
    public void HighlightTokens(IEnumerable<Token> tokens, TextWriter writer)
    {
        const int tabLength = 4;
        var line = 0;
        writer.WriteText($"\r\n{(line + 1).ToString().PadLeft(5, '0')}: ", ConsoleColor.Green);

        void processMultiLineToken(Token token, ConsoleColor color)
        {
            var lines = token.Text.Replace("\r", "\n").Replace("\n\n", "\n").Split('\n');
            var first = true;
            foreach (var l in lines)
            {
                if (first) first = false;
                else
                {
                    line++;
                    writer.WriteText($"\r\n{(line + 1).ToString().PadLeft(5, '0')}: ", ConsoleColor.Green);
                }

                var transformed = l.Replace(' ', '·').Replace("\t", new string('·', tabLength));
                writer.Write(transformed, color);
            }
        }

        foreach (var token in tokens)
        {
            // Let's highlight a bit...
            switch (token.Kind)
            {
                case TokenKind.Invalid:
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    writer.WriteText($"{token.Text}", ConsoleColor.Red);
                    break;
                case TokenKind.Eof:
                    // Always write Eof
                    writer.WriteLine();
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    writer.WriteText("EOF", ConsoleColor.Cyan);
                    break;
                // Operators
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Star:
                case TokenKind.Slash:
                case TokenKind.Percent:
                case TokenKind.OpenParenthesis:
                case TokenKind.CloseParenthesis:
                case TokenKind.OpenBrace:
                case TokenKind.CloseBrace:
                case TokenKind.Lower:
                case TokenKind.LowerEqual:
                case TokenKind.Greater:
                case TokenKind.GreaterEqual:
                case TokenKind.Exclamation:
                case TokenKind.ExclamationEqual:
                case TokenKind.Equal:
                case TokenKind.EqualEqual:
                case TokenKind.DoubleQuote:
                case TokenKind.Colon:
                case TokenKind.Comma:
                case TokenKind.Semicolon:
                    writer.WriteText($"{token.Text}", ConsoleColor.Gray);
                    break;
                case TokenKind.IntLiteral:
                case TokenKind.DoubleLiteral:
                case TokenKind.StringLiteral:
                    writer.WriteText($"{token.Text}", ConsoleColor.DarkRed);
                    break;
                case TokenKind.Identifier:
                    writer.WriteText($"{token.Text}", ConsoleColor.DarkMagenta);
                    break;
                case TokenKind.Whitespace:
                    processMultiLineToken(token, ConsoleColor.Green);
                    break;
                case TokenKind.Comment:
                    processMultiLineToken(token, ConsoleColor.DarkGray);
                    break;
                default:
                    if (token.IsKeyword())
                        writer.WriteText($"{token.Text}", ConsoleColor.Cyan);
                    else
                        writer.Write(token);
                    break;
            }
        }

        writer.WriteLine();
    }
}
