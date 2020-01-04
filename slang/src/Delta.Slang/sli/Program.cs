using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Delta.Slang.Syntax;
using Delta.Slang.Utils;

namespace Delta.Slang.Repl
{
    internal class Program
    {
        static Program()
        {
            Writer = new ConsoleOutputWriter();
            NullWriter = new NullOutputWriter();
        }

        private static int Main(string[] args)
        {
            var rc = new Program().Run(args);
            return rc;
        }

        private static IOutputWriter Writer { get; }
        private static IOutputWriter NullWriter { get; }

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

        private int RunFile(string filename, bool noOutput = false)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", filename);
            Writer.WriteLine($"Loading {path}");

            var watch = new Stopwatch();

            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                watch.Reset();
                watch.Start();
                ProcessInput(reader, noOutput);
                watch.Stop();
            }

            Console.WriteLine($"Processing time: {TimeSpan.FromTicks(watch.ElapsedTicks)}");
            return 0;
        }

        private int RunInteractive()
        {
            Writer.WriteLine("Press ^C to end the program");

            var exit = false;
            while (!exit)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
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
            Console.WriteLine("Q - Quit");
            Console.WriteLine("E - Enter a program (2 blank lines to end input)");
        }

        private void ProcessInteractiveProgram()
        {
            var emptyLineCounter = 0;
            var buffer = new StringBuilder();

            while (true)
            {
                Console.Write(": ");
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    emptyLineCounter++;

                _ = buffer.AppendLine(line);

                if (emptyLineCounter == 2)
                {
                    ProcessInput(buffer.ToString(), false);
                    _ = buffer.Clear();
                    break;
                }
            }
        }

        private void ProcessInput(TextReader reader, bool noOutput)
        {
            using (var interpreter = new Interpreter(reader))
                ProcessInput(interpreter, noOutput);
        }

        private void ProcessInput(string source, bool noOutput)
        {
            using (var interpreter = new Interpreter(source))
                ProcessInput(interpreter, noOutput);
        }

        private void ProcessInput(Interpreter interpreter, bool noOutput)
        {
            var writer = noOutput ? NullWriter : Writer;
            Highlight(interpreter, writer);

            writer.WriteLine();

            // And now, parse
            var (t, d) = interpreter.Parse();
            writer.WriteLine("Parse Tree: ");
            Walk(t.Root, writer);
            writer.WriteLine();
            if (d.Any())
            {
                writer.WriteLine("Diagnostics: ");
                foreach (var diagnostic in d)
                    writer.WriteLine(diagnostic);
            }
            else writer.WriteLine("Diagnostics: None");

            writer.WriteLine();
            writer.WriteLine("Unparse: ");
            Unparse(t, writer);

            writer.WriteLine();
            writer.WriteLine("Compile: ");
            Compile(t, writer);
        }

        private void Highlight(Interpreter interpreter, IOutputWriter writer)
        {
            var previousLine = -1;

            foreach (var (line, token) in interpreter.LexWithLineNumbers())
            {
                if (line != previousLine)
                {
                    previousLine = line;
                    writer.Write($"\r\n{(line + 1).ToString().PadLeft(5, '0')}: ", ConsoleColor.Green);
                }

                // Let's highlight a bit...
                switch (token.Kind)
                {
                    case TokenKind.Invalid:
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        writer.Write($"{token.Text}", ConsoleColor.Red);
                        break;
                    case TokenKind.Eof:
                        // Always write Eof
                        Writer.WriteLine();
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Writer.Write("EOF", ConsoleColor.Cyan);
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
                        writer.Write($"{token.Text}", ConsoleColor.Gray);
                        break;
                    case TokenKind.NumberLiteral:
                    case TokenKind.StringLiteral:
                        writer.Write($"{token.Text}", ConsoleColor.DarkRed);
                        break;
                    case TokenKind.Identifier:
                        writer.Write($"{token.Text}", ConsoleColor.DarkMagenta);
                        break;
                    case TokenKind.Whitespace:
                        writer.Write($"{new string('·', token.Text.Replace("\r", "").Replace("\n", "").Length)}", ConsoleColor.DarkGray);
                        break;
                    case TokenKind.Comment:
                        writer.Write($"{token.Text}", ConsoleColor.DarkGray);
                        break;
                    default:
                        if (token.IsKeyword())
                            writer.Write($"{token.Text}", ConsoleColor.Cyan);
                        else
                            writer.Write(token);
                        break;
                }
            }

            writer.WriteLine();
        }

        private void Unparse(ParseTree t, IOutputWriter writer)
        {
            var unparsed = "";
            using (var tempWriter = new StringWriter())
            {
                new Unparser(t).Unparse(tempWriter);
                unparsed = tempWriter.ToString();
            }

            using (var interpreter = new Interpreter(unparsed))
                Highlight(interpreter, writer);
        }

        private void Compile(ParseTree t, IOutputWriter writer)
        {
            var compilation = new Compilation(t);
            compilation.EmitTree(writer.TextWriter);
        }

        private void Walk(SyntaxNode node, IOutputWriter writer, int tabCount = 0)
        {
            string f(Token token)
            {
                if (token == null) return "<NULL>";
                var text = token.Value == null ? token.Text ?? "" : token.Value.ToString();
                return text.Replace("\r", "\\r").Replace("\n", "\\n").Replace(" ", "·");
            }

            var tabs = new string(' ', tabCount * 3);
            writer.WriteLine($"{tabs}{node.Kind} - {f(node.MainToken)}");
            foreach (var child in node.Children)
                Walk(child, writer, tabCount + 1);
        }

        private static void LogError(object text) => Writer.WriteLine($"ERROR - {text ?? "<NULL>"}", ConsoleColor.Red);

        #region Write*

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

        #endregion
    }
}
