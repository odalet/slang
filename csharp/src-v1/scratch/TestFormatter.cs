using System;

namespace Scratch
{
    internal sealed class TestFormatter
    {
        private const int tabSize = 4;
        private static readonly string nl = Environment.NewLine;
        private int indentLevel;

        private string Indentation => new(' ', tabSize * indentLevel);
        private string Fix(string text) => $"{Indentation}{text}";
        private string Fixln(string text) => $"{Fix(text)}{nl}";
        private void Indent() => indentLevel++;
        private void Dedent() => indentLevel = --indentLevel < 0 ? 0 : indentLevel;

        public void Test()
        {
            var text = Fixln($"{indentLevel}");
            Indent();
            text += Fixln($"{indentLevel}");
            Indent();
            text += Fixln($"{indentLevel}");
            Dedent();
            text += Fixln($"{indentLevel}");
            Dedent();
            text += Fixln($"{indentLevel}");
            Dedent();
            text += Fixln($"{indentLevel}");

            Console.WriteLine(text);
        }
    }
}
