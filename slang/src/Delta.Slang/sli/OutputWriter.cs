using System;

namespace Delta.Slang.Repl
{
    internal interface IOutputWriter
    {
        void Write(object text);
        void WriteLine(object text);
        void WriteLine();
        void Write(object text, ConsoleColor color);
        void WriteLine(object text, ConsoleColor color);
    }

    internal class NullOutputWriter : IOutputWriter
    {
        public void Write(object text) { /* Empty by design */ }
        public void Write(object text, ConsoleColor color) { /* Empty by design */ }
        public void WriteLine(object text) { /* Empty by design */ }
        public void WriteLine() { /* Empty by design */ }
        public void WriteLine(object text, ConsoleColor color) { /* Empty by design */ }
    }

    internal class ConsoleOutputWriter : IOutputWriter
    {
        public void Write(object text) => Console.Write(text);
        public void WriteLine(object text) => Console.WriteLine(text);
        public void WriteLine() => Console.WriteLine();
        public void Write(object text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        public void WriteLine(object text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
