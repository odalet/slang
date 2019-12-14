using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Delta.Slang.Syntax;

namespace Delta.Slang
{
    public sealed class Interpreter : IDisposable
    {
        public Interpreter(TextReader reader)
        {
            var input = reader ?? throw new ArgumentNullException(nameof(reader));

            var sourceText = SourceText.From(input, Encoding.UTF8);
            Lexer = new Lexer(sourceText);
        }

        private Lexer Lexer { get; }

        public IEnumerable<IDiagnostic> Diagnostics => Lexer.Diagnostics;

        public IEnumerable<(int, Token)> Lex() => Lexer.Lex();

        public void Dispose() => Lexer.Dispose();
    }
}
