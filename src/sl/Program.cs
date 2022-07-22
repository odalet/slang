using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Slang.CodeAnalysis;
using Slang.CodeAnalysis.Syntax;
using Slang.CodeAnalysis.Text;

namespace Slang.Cli
{
    internal sealed class Program
    {
        private enum ExitCode
        {
            // See https://www.freebsd.org/cgi/man.cgi?query=sysexits&apropos=0&sektion=0&manpath=FreeBSD+4.3-RELEASE&format=html
            OK = 0,
            InvalidUsage = 64,
            DataError = 65,
            NoInput = 66
        }

        public Program()
        {
            Reader = Console.In;
            Writer = Console.Out;
            ErrWriter = Console.Error;
        }

        private static int Main(string[] args) => (int)new Program().Run(args);

        private TextReader Reader { get; }
        private TextWriter Writer { get; }
        private TextWriter ErrWriter { get; }

        private ExitCode Run(string[] args)
        {
            var exeName = Process.GetCurrentProcess().ProcessName;
            if (args.Length > 1)
            {
                ErrWriter.WriteLine($"Usage: {exeName} [script]");
                return ExitCode.InvalidUsage;
            }

            if (args.Length == 1)
            {
                var filename = args[0];
                if (!Path.IsPathRooted(filename))
                    filename = Path.Combine(Environment.CurrentDirectory, filename);

                if (!File.Exists(filename))
                {
                    ErrWriter.WriteLine($"File not found: {filename}");
                    return ExitCode.NoInput;
                }

                return RunFile(filename);
            }

            return RunCli();
        }

        private ExitCode RunFile(string filename)
        {
            Writer.WriteLine($"> Loading {filename}");
            var watch = new Stopwatch();

            using var reader = new StreamReader(File.OpenRead(filename));
            watch.Reset();
            watch.Start();
            var diagnostics = new DiagnosticCollection();
            ProcessInput(reader, diagnostics);
            watch.Stop();

            Writer.WriteLine($"> Processing time: {TimeSpan.FromTicks(watch.ElapsedTicks)}");
            return ExitCode.OK;
        }

        private ExitCode RunCli()
        {
            Writer.WriteLine("Enter ;; to exit");

            const string prompt = "sl> ";

            while (true)
            {
                Writer.Write(prompt);
                var line = Reader.ReadLine() ?? "";
                if (line.Trim() == ";;")
                    break;

                // One new collection per read line
                var diagnostics = new DiagnosticCollection();
                ProcessInput(line, diagnostics);
            }

            return ExitCode.OK;
        }

        private void ProcessInput(TextReader reader, DiagnosticCollection diagnostics) => ProcessInput(SourceText.From(reader, Encoding.UTF8), diagnostics);
        private void ProcessInput(string source, DiagnosticCollection diagnostics) => ProcessInput(SourceText.From(source, Encoding.UTF8), diagnostics);
        private void ProcessInput(SourceText sourceText, DiagnosticCollection diagnostics)
        {
            Writer.WriteLine($"> Processing '{sourceText}'");

            var lexer = new Lexer(sourceText, diagnostics);
            //foreach (var token in lexer.Lex())
            //    Writer.WriteLine($"-> {token} - {token.Position}");

            var tokens = lexer.Lex();

            var parser = new Parser(tokens, diagnostics);
            var tree = parser.Parse();

            Writer.WriteLine($"-> Tree:\r\n\r\n{PrettyPrint(tree.Root.ToString())}");

            foreach (var diagnostic in diagnostics)
                Writer.WriteLine(diagnostic);

            Writer.WriteLine($"< Done Processing '{sourceText}'");
        }

        private static string PrettyPrint(string text)
        {
            const int tabSize = 4;
            var tabs = 0;
            var output = new StringBuilder();
            var index = 0;

            void outputTabs() => output.Append(new string(' ', tabSize * tabs));
            void eatWhitespaces()
            {
                while (char.IsWhiteSpace(text[index + 1]) && index < text.Length - 1)
                    index++;
            }

            while (true)
            {
                var c = text[index];
                switch (c)
                {
                    case '{':
                        _ = output.Append('{');
                        _ = output.Append(Environment.NewLine);
                        tabs++;
                        outputTabs();
                        eatWhitespaces();
                        break;
                    case '}':
                        _ = output.Append(Environment.NewLine);
                        tabs = tabs-- < 0 ? 0 : tabs;
                        outputTabs();
                        _ = output.Append('}');
                        break;
                    case ',':
                        _ = output.Append(',');
                        _ = output.Append(Environment.NewLine);
                        outputTabs();
                        eatWhitespaces();
                        break;
                    default:
                        _ = output.Append(c);
                        break;
                }

                index++;
                if (index >= text.Length)
                    break;
            }

            return output.ToString();
        }
    }
}
