using System;
using System.IO;
using System.Linq;
using System.Text;
using Slang.CodeAnalysis;
using Slang.CodeAnalysis.Syntax;
using Slang.CodeAnalysis.Text;

namespace Slang
{
    internal static class Helper
    {
        public static (Token[] tokens, DiagnosticCollection diagnostics) Lex(string source)
        {
            var diagnostics = new DiagnosticCollection();
            var lexer = new Lexer(SourceText.From(source), diagnostics);
            var tokens = lexer.Lex().ToArray();
            return (tokens, diagnostics);
        }

        public static (Token[] tokens, DiagnosticCollection diagnostics) LexFile(string filename)
        {
            var diagnostics = new DiagnosticCollection();

            using var reader = new StreamReader(File.OpenRead(filename));

            var lexer = new Lexer(SourceText.From(reader, Encoding.UTF8), diagnostics);
            var tokens = lexer.Lex().ToArray();
            return (tokens, diagnostics);
        }

        public static string Resolve(string filename) =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", filename);
    }
}
