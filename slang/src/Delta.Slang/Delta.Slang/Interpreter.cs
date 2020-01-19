using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Delta.Slang.Semantics;
using Delta.Slang.Syntax;

namespace Delta.Slang
{
    [Flags]
    [SuppressMessage("Design", "RCS1157:Composite enum value contains undefined flag.", Justification = "Cannot Parse without lexing or Bind without parsing")]
    [SuppressMessage("Major Code Smell", "S4070:Non-flags enums should not be marked with \"FlagsAttribute\"", Justification = "This is a flags enum!")]
    public enum InterpreterRunOptions : byte
    {
        None = 0,
        Lex = 1,
        Parse = 3,
        Bind = 7,
        All = 0xFF
    }

    public static class RunOptionsExtensions
    {
        public static bool ShouldLex(this InterpreterRunOptions options) => (options & InterpreterRunOptions.Lex) == InterpreterRunOptions.Lex;
        public static bool ShouldParse(this InterpreterRunOptions options) => (options & InterpreterRunOptions.Parse) == InterpreterRunOptions.Parse;
        public static bool ShouldBind(this InterpreterRunOptions options) => (options & InterpreterRunOptions.Bind) == InterpreterRunOptions.Bind;
    }

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

        public Result Run(InterpreterRunOptions options = InterpreterRunOptions.All)
        {
            var diagnostics = new DiagnosticCollection();

            var tokens = options.ShouldLex() ? Lex(diagnostics) : null;
            var parseTree = options.ShouldParse() ? Parse(tokens, diagnostics) : null;
            var boundTree = options.ShouldBind() && parseTree != null ? Bind(parseTree, diagnostics) : null;

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
