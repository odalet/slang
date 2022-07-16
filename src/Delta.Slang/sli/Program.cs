using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Delta.Slang.Semantics;
using Delta.Slang.Syntax;
using Delta.Slang.Utils;

namespace Delta.Slang.Repl
{
    internal class Program
    {
        static Program() => Writer = Console.Out;

        private static int Main(string[] args)
        {
            var rc = new Program().Run(args);
            return rc;
        }

        private static TextWriter Writer { get; }

        private Interpreter.Result LastResult { get; set; }

        private int Run(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    var filename = args[0];
                    return RunFile(filename);
                }

                // Otherwise
                return RunInteractive();
            }
            catch (Exception ex)
            {
                LogError($"Fatal Error: {ex.Message}");
                return -1;
            }
        }

        private int RunFile(string filename)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", filename);
            Writer.WriteLine($"Loading {path}");

            var watch = new Stopwatch();

            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                watch.Reset();
                watch.Start();
                ProcessInput(reader);
                watch.Stop();
            }

            Writer.WriteLine($"Processing time: {TimeSpan.FromTicks(watch.ElapsedTicks)}");
            return 0;
        }

        private int RunInteractive()
        {
            Writer.WriteLine("Press ^C to end the program");

            var exit = false;
            while (!exit)
            {
                Writer.Write("> ");
                var input = Console.ReadLine().Replace("\t", "    ");
                switch (input.ToLowerInvariant().Trim())
                {
                    case "q":
                        exit = true;
                        break;
                    case "e":
                        ProcessInteractiveProgram();
                        break;
                    default:
                        ShowHelp();
                        break;
                }
            }

            return 0;
        }

        private void ShowHelp()
        {
            Writer.WriteLine("Q - Quit");
            Writer.WriteLine("E - Enter a program (;; on its own line to end input)");
        }

        private void ProcessInteractiveProgram()
        {
            var buffer = new StringBuilder();
            while (true)
            {
                Writer.Write(": ");
                var line = Console.ReadLine();
                if (line.Trim() == ";;")
                {
                    ProcessInput(buffer.ToString());
                    _ = buffer.Clear();
                    break;
                }
                _ = buffer.AppendLine(line);
            }
        }

        private void ProcessInput(TextReader reader) => ProcessInput(SourceText.From(reader, Encoding.UTF8));
        private void ProcessInput(string source) => ProcessInput(SourceText.From(source, Encoding.UTF8));

        private void ProcessInput(SourceText sourceText)
        {
            var interpreter = new Interpreter(sourceText);
            LastResult = interpreter.Run();

            Writer.WriteLine();
            Writer.WriteLine("Diagnostics: ");
            ShowDiagnostics(LastResult.Diagnostics);

            Writer.WriteLine();
            Writer.WriteLine("Highlighted tokens: ");
            new Highlighter().HighlightTokens(LastResult.Tokens, Writer);

            Writer.WriteLine();
            Writer.WriteLine("Parse Tree: ");
            ShowParseTree(LastResult.ParseTree);

            Writer.WriteLine();
            Writer.WriteLine("Highlighted Unparsed Tree: ");
            Unparse(LastResult.ParseTree);

            Writer.WriteLine();
            Writer.WriteLine("Bound Tree: ");
            ShowBoundTree(LastResult.BoundTree);
        }

        private void ShowDiagnostics(IEnumerable<IDiagnostic> diagnostics)
        {
            if (diagnostics.Any())
            {
                foreach (var diagnostic in diagnostics)
                    Writer.WriteLine(diagnostic);
            }
            else Writer.WriteLine("--> None");
        }

        private void ShowParseTree(ParseTree tree)
        {
            if (tree == null)
            {
                Writer.WriteLine("No Parse Tree");
                return;
            }

            new Unparser(tree).DumpTree(Writer);
        }
        
        private void Unparse(ParseTree tree)
        {
            if (tree == null)
            {
                Writer.WriteLine("No Parse Tree");
                return;
            }

            var unparsed = "";
            using (var tempWriter = new StringWriter())
            {
                new Unparser(tree).Unparse(tempWriter);
                unparsed = tempWriter.ToString();
            }

            var localInterpreter = new Interpreter(SourceText.From(unparsed, Encoding.UTF8));
            var localResults = localInterpreter.Run(InterpreterRunOptions.Parse);
            if (localResults.Diagnostics.Any())
            {
                foreach (var diagnostic in localResults.Diagnostics)
                    Writer.WriteLine(diagnostic);
            }

            new Highlighter().HighlightTokens(localResults.Tokens, Writer);
        }

        private void ShowBoundTree(BoundTree tree)
        {
            if (tree == null)
            {
                Writer.WriteLine("No Bound Tree");
                return;
            }

            if (tree.Statements.Any())
            {
                foreach (var statement in tree.Statements)
                    statement.WriteTo(Writer);
            }

            if (tree.Functions.Any())
            {
                foreach (var function in tree.Functions)
                    function.WriteTo(Writer);
            }

            Writer.WriteLine();
        }

        private static void LogError(object text) => Writer.WriteText($"ERROR - {text ?? "<NULL>"}\r\n", ConsoleColor.Red);
    }
}
