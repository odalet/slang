using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Delta.Slang.Semantic;
using Delta.Slang.Syntax;

namespace Delta.Slang
{
    [ExcludeFromCodeCoverage]
    internal static class Helpers
    {
        public static (IEnumerable<Token> tokens, IEnumerable<IDiagnostic> diagnostics) Lex(string text) => Lex(SourceText.From(text));
        public static (IEnumerable<Token> tokens, IEnumerable<IDiagnostic> diagnostics) Lex(SourceText source)
        {
            using (var lexer = new Lexer(source))
            {
                var tokens = lexer.Lex();
                return (tokens, lexer.Diagnostics);
            }
        }

        public static (ParseTree tree, IEnumerable<IDiagnostic> diagnostics) Parse(string text) => Parse(SourceText.From(text));
        public static (ParseTree tree, IEnumerable<IDiagnostic> diagnostics) Parse(SourceText source)
        {
            using (var lexer = new Lexer(source))
            {
                var tokens = lexer.Lex();
                var parser = new Parser(tokens);
                var tree = parser.Parse();

                return (
                    tree,
                    new DiagnosticCollection().AddRange(lexer.Diagnostics).AddRange(parser.Diagnostics));
            }
        }

        public static (BoundTree tree, IEnumerable<IDiagnostic> diagnostics) Bind(string text) => Bind(SourceText.From(text));
        public static (BoundTree tree, IEnumerable<IDiagnostic> diagnostics) Bind(SourceText source)
        {
            var parserResults = Parse(source);
            var boundTree = Binder.BindCompilationUnit(parserResults.tree.Root);
            return (
                boundTree, 
                new DiagnosticCollection().AddRange(parserResults.diagnostics).AddRange(boundTree.Diagnostics));
        }
    }
}
