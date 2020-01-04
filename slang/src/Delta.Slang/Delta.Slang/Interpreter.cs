using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Delta.Slang.Semantic;
using Delta.Slang.Syntax;

namespace Delta.Slang
{
    public sealed class Interpreter
    {
        public sealed class Result
        {
            public IEnumerable<Token> Tokens { get; internal set; }
            public ParseTree ParseTree { get; internal set; }
            public BoundTree BoundTree { get; internal set; }
            public IEnumerable<IDiagnostic> Diagnostics { get; internal set; }
        }

        public Interpreter(SourceText sourceText) => SourceText = sourceText ?? throw new ArgumentNullException(nameof(sourceText));

        private SourceText SourceText { get; }

        public Result Run()
        {
            var diagnostics = new DiagnosticCollection();

            var tokens = Lex(diagnostics);
            var parseTree = Parse(tokens, diagnostics);
            var boundTree = parseTree != null ? Bind(parseTree, diagnostics) : null;

            return new Result
            {
                Tokens = tokens ?? new Token[0],
                ParseTree = parseTree,
                BoundTree = boundTree,
                Diagnostics = diagnostics
            };
        }

        private Token[] Lex(DiagnosticCollection diagnostics)
        {
            using (var lexer = new Lexer(SourceText))
            {
                try
                {
                    var lexed = lexer.Lex();
                    var tokens = lexed.ToArray();
                    _ = diagnostics.AddRange(lexer.Diagnostics);
                    return tokens;
                }
                catch (Exception ex)
                {
                    diagnostics.ReportLexerException(ex);
                }

                return new Token[0];
            }
        }

        private ParseTree Parse(Token[] tokens, DiagnosticCollection diagnostics)
        {
            try
            {
                var parser = new Parser(tokens);
                var tree = parser.Parse();
                _ = diagnostics.AddRange(parser.Diagnostics);
                return tree;
            }
            catch (Exception ex)
            {
                diagnostics.ReportParserException(ex);
            }

            return null;
        }

        private BoundTree Bind(ParseTree parseTree, DiagnosticCollection diagnostics)
        {
            try
            {
                var tree = Binder.BindCompilationUnit(parseTree.Root);
                _ = diagnostics.AddRange(tree.Diagnostics);
                return tree;
            }
            catch (Exception ex)
            {
                diagnostics.ReportBinderException(ex);
            }

            return null;
        }
    }
}
