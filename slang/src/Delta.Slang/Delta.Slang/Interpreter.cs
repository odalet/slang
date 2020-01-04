using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Delta.Slang.Syntax;

namespace Delta.Slang
{
    public sealed class Interpreter : IDisposable
    {
        public Interpreter(TextReader reader) : this(SourceText.From(reader, Encoding.UTF8)) { }

        public Interpreter(string source) : this(SourceText.From(source)) { }

        private Interpreter(SourceText sourceText)
        {
            SourceText = sourceText;
            Lexer = new Lexer(SourceText);
        }

        private SourceText SourceText { get; }

        private Lexer Lexer { get; }

        public IEnumerable<IDiagnostic> Diagnostics => Lexer.Diagnostics;

        public IEnumerable<(int, Token)> LexWithLineNumbers() => Lexer.LexWithLineNumbers();

        public (ParseTree tree, IEnumerable<IDiagnostic> diagnostics) Parse()
        {
            using (var lexer = new Lexer(SourceText))
            {
                var tokens = lexer.Lex();
                var parser = new Parser(tokens);
                var tree = parser.Parse();
                var diagnostics = new DiagnosticCollection()
                    .AddRange(lexer.Diagnostics)
                    .AddRange(parser.Diagnostics);

                return (tree, diagnostics);
            }
        }

        public void Compile(TextWriter writer)
        {
            var (t, d) = Parse();
            var compilation = new Compilation(t);
            compilation.EmitTree(writer);
        }

        public void Dispose() => Lexer.Dispose();
    }
}
