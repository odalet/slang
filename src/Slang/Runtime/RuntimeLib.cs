using System.IO;

namespace Slang.Runtime
{
    public interface IRuntimeLib
    {
        void Print(RuntimeValue value, bool appendNewLine);
    }

    public sealed class RuntimeLib : IRuntimeLib
    {
        private readonly TextReader stdin;
        private readonly TextWriter stdout;
        private readonly TextWriter stderr;

        public RuntimeLib(TextReader inReader, TextWriter outWriter, TextWriter errWriter)
        {
            stdin = inReader;
            stdout = outWriter;
            stderr = errWriter;
        }

        public void Print(RuntimeValue value, bool appendNewLine)
        {
            object text = value.IsString(out var s) ? Unescape(s) : value;
            if (appendNewLine)
                stdout.WriteLine(text);
            else
                stdout.Write(text);
        }

        private static string Unescape(string text) => text
            .Replace("\\t", "\t")
            .Replace("\\r", "\r")
            .Replace("\\n", "\n")
            ;
    }
}
