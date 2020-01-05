using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Delta.Slang.Semantic;
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
            HighlightTokens(LastResult.Tokens);

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

            void walk(SyntaxNode node, TextWriter writer, int tabCount = 0)
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
                    walk(child, writer, tabCount + 1);
            }

            walk(tree.Root, Writer);
        }
        
        private void HighlightTokens(IEnumerable<Token> tokens)
        {
            const int tabLength = 4;
            var line = 0;
            Writer.WriteText($"\r\n{(line + 1).ToString().PadLeft(5, '0')}: ", ConsoleColor.Green);

            void processMultiLineToken(Token token, ConsoleColor color)
            {
                var lines = token.Text.Replace("\r", "\n").Replace("\n\n", "\n").Split('\n');
                var first = true;
                foreach (var l in lines)
                {
                    if (first) first = false;
                    else
                    {
                        line++;
                        Writer.WriteText($"\r\n{(line + 1).ToString().PadLeft(5, '0')}: ", ConsoleColor.Green);
                    }

                    var transformed = l.Replace(' ', '·').Replace("\t", new string('·', tabLength));
                    Writer.Write(transformed, color);
                }
            }

            foreach (var token in tokens)
            {
                // Let's highlight a bit...
                switch (token.Kind)
                {
                    case TokenKind.Invalid:
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Writer.WriteText($"{token.Text}", ConsoleColor.Red);
                        break;
                    case TokenKind.Eof:
                        // Always write Eof
                        Writer.WriteLine();
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                        Writer.WriteText("EOF", ConsoleColor.Cyan);
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
                        Writer.WriteText($"{token.Text}", ConsoleColor.Gray);
                        break;
                    case TokenKind.NumberLiteral:
                    case TokenKind.StringLiteral:
                        Writer.WriteText($"{token.Text}", ConsoleColor.DarkRed);
                        break;
                    case TokenKind.Identifier:
                        Writer.WriteText($"{token.Text}", ConsoleColor.DarkMagenta);
                        break;
                    case TokenKind.Whitespace:
                        processMultiLineToken(token, ConsoleColor.Green);
                        break;
                    case TokenKind.Comment:
                        processMultiLineToken(token, ConsoleColor.DarkGray);
                        break;
                    default:
                        if (token.IsKeyword())
                            Writer.WriteText($"{token.Text}", ConsoleColor.Cyan);
                        else
                            Writer.Write(token);
                        break;
                }
            }

            Writer.WriteLine();
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

            HighlightTokens(localResults.Tokens);
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
