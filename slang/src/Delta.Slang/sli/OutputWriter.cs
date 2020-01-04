using System;
using System.IO;
using System.Text;

namespace Delta.Slang.Repl
{
    internal interface IOutputWriter
    {
        TextWriter TextWriter { get; }
        void Write(object text);
        void WriteLine(object text);
        void WriteLine();
        void Write(object text, ConsoleColor color);
        void WriteLine(object text, ConsoleColor color);
    }

    internal class NullOutputWriter : IOutputWriter
    {
        private class NullTextWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        public TextWriter TextWriter { get; } = new NullTextWriter();

        public void Write(object text) { /* Empty by design */ }
        public void Write(object text, ConsoleColor color) { /* Empty by design */ }
        public void WriteLine(object text) { /* Empty by design */ }
        public void WriteLine() { /* Empty by design */ }
        public void WriteLine(object text, ConsoleColor color) { /* Empty by design */ }
    }

    internal class ConsoleOutputWriter : IOutputWriter
    {
        public TextWriter TextWriter => Console.Out;

        public void Write(object text) => TextWriter.Write(text);
        public void WriteLine(object text) => TextWriter.WriteLine(text);
        public void WriteLine() => TextWriter.WriteLine();
        public void Write(object text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            TextWriter.Write(text);
            Console.ResetColor();
        }

        public void WriteLine(object text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            TextWriter.WriteLine(text);
            Console.ResetColor();
        }
    }
}
