using System;
using System.Diagnostics;
using System.IO;
using Delta.Slang.Syntax;

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

#if DEBUG
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
#endif
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
            Writer.WriteLine("Press ^Z to end the program");
            ProcessInput(Console.In, false);
            return 0;
        }

        private void ProcessInput(TextReader reader, bool noOutput)
        {
            var w = noOutput ? NullWriter : Writer;

            using var interpreter = new Interpreter(reader);
            var previousLine = -1;
            ////var first = true;
            foreach (var (line, token) in interpreter.Lex())
            {
                if (line != previousLine)
                {
                    ////first = true;
                    previousLine = line;
                    w.Write($"\r\n{line.ToString().PadLeft(5, '0')}: ", ConsoleColor.Green);
                }

                ////if (first) first = false;
                ////else if (token.Kind != TokenKind.Eol) w.Write("");

                // Let's highlight a bit...
                switch (token.Kind)
                {
                    case TokenKind.Invalid:
                        w.Write($"{token.Text}", ConsoleColor.Red);
                        break;
                    case TokenKind.Eol:
                        w.Write("$", ConsoleColor.Cyan);
                        w.WriteLine();
                        break;
                    case TokenKind.Eof:
                        // Always write Eof
                        Writer.WriteLine();
                        Writer.Write("EOF", ConsoleColor.Cyan);
                        break;
                    case TokenKind.OpenParenthesis:
                    case TokenKind.CloseParenthesis:
                    case TokenKind.DoubleQuote:
                    case TokenKind.Semicolon:
                        w.Write($"{token.Text}", ConsoleColor.Yellow);
                        break;
                    case TokenKind.StringLiteral:
                        w.Write($"{token.Text}", ConsoleColor.White);
                        break;
                    case TokenKind.Identifier:
                        w.Write($"{token.Text}", ConsoleColor.Magenta);
                        break;
                    case TokenKind.Whitespace:
                        w.Write($"{new string('·', token.Text.Replace("\r", "").Replace("\n", "").Length)}", ConsoleColor.DarkGray);
                        break;
                    case TokenKind.Comment:
                        w.Write($"/* {token.Text} */", ConsoleColor.DarkGray);
                        break;
                    default:
                        w.Write(token);
                        break;
                }
            }

            Writer.WriteLine();
            foreach (var diagnostic in interpreter.Diagnostics)
                Writer.WriteLine(diagnostic);
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
