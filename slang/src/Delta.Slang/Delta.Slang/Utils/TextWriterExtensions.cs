using System;
using System.CodeDom.Compiler;
using System.IO;
using Delta.Slang.Syntax;

namespace Delta.Slang.Utils
{
    public static class TextWriterExtensions
    {
        private const ConsoleColor identifierColor = ConsoleColor.DarkYellow;
        private const ConsoleColor typeIdentifierColor = ConsoleColor.Yellow;
        private const ConsoleColor keywordColor = ConsoleColor.Blue;
        private const ConsoleColor punctuationColor = ConsoleColor.DarkGray;
        private const ConsoleColor numberColor = ConsoleColor.Cyan;
        private const ConsoleColor stringColor = ConsoleColor.Magenta;

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetForeground(identifierColor);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteTypeIdentifier(this TextWriter writer, string text)
        {
            writer.SetForeground(typeIdentifierColor);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteKeyword(this TextWriter writer, TokenKind kind) => writer.WriteKeyword(kind.GetText());

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetForeground(keywordColor);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetForeground(numberColor);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.SetForeground(stringColor);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WritePunctuation(this TextWriter writer, TokenKind kind) => writer.WritePunctuation(kind.GetText());

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetForeground(punctuationColor);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteSpace(this TextWriter writer) => writer.WritePunctuation(" ");

        public static void WriteText(this TextWriter writer, string text, ConsoleColor color)
        {
            writer.SetForeground(color);
            writer.Write(text);
            writer.ResetColor();
        }

        private static bool IsConsoleOut(this TextWriter writer) => writer == Console.Out || writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut();

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsoleOut()) Console.ForegroundColor = color;
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsoleOut()) Console.ResetColor();
        }
    }
}
