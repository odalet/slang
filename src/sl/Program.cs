using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Slang.CodeAnalysis;
using Slang.CodeAnalysis.Syntax;
using Slang.CodeAnalysis.Text;
using Slang.Utilities;

namespace Slang.Cli
{
    internal sealed class Program
    {
        private static readonly string nl = Environment.NewLine;

        private static bool shouldDumpTokens = false;
        private static bool shouldDumpParseTree = true;

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
            var tokens = lexer.Lex().ToArray();
            if (shouldDumpTokens)
            {
                foreach (var token in tokens)
                    Writer.WriteLine($"-> {token} - {token.Position}");
            }

            var parser = new Parser(tokens, diagnostics);
            var tree = parser.Parse();
            if (shouldDumpParseTree)
                Writer.WriteLine($"-> Tree:{nl}{nl}{Dump(tree)}");

            foreach (var diagnostic in diagnostics)
                Writer.WriteLine(diagnostic);

            var dumper = new MarkdownDumper(tokens, tree, diagnostics);
            dumper.DumpTo(@"c:\temp\dump.md");

            Writer.WriteLine($"< Done Processing '{sourceText}'");
        }

        private static string Dump(ParseTree tree) => new ParseTreePrettyPrinter(tree).Dump();
    }
}
