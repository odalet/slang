using System;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using Slang.CodeAnalysis;
using Slang.CodeAnalysis.Syntax;
using Slang.CodeAnalysis.Text;
using Slang.Runtime;

namespace Slang.Interpreter;

internal static class Helpers
{
    private sealed class DisposableRuntime : IDisposable
    {
        private readonly TextReader inReader;
        private readonly TextWriter outWriter;
        private readonly TextWriter errWriter;

        public DisposableRuntime(StringBuilder outBuilder, StringBuilder errBuilder)
        {
            inReader = new StringReader("");
            outWriter = new StringWriter(outBuilder);
            errWriter = new StringWriter(errBuilder);

            RuntimeLib = new RuntimeLib(inReader, outWriter, errWriter);
        }

        public RuntimeLib RuntimeLib { get; }

        public void Dispose()
        {
            errWriter.Dispose();
            outWriter.Dispose();
            inReader.Dispose();
        }
    }

    public static string Interpret(string source) => Interpret(source, out _, out _);
    public static string Interpret(string source, out DiagnosticCollection diagnostics) => Interpret(source, out diagnostics, out _);
    public static string Interpret(string source, out DiagnosticCollection diagnostics, out string err)
    {
        diagnostics = new DiagnosticCollection();
        var sourceText = SourceText.From(source, Encoding.UTF8);
        var lexer = new Lexer(sourceText, diagnostics);
        var tokens = lexer.Lex().ToArray();

        var parser = new Parser(tokens, diagnostics);
        var tree = parser.Parse();

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        using var runtime = MakeForTests(stdout, stderr);
        using var interpreter = new ParseTreeInterpreter(tree, runtime.RuntimeLib);
        _ = interpreter.Execute();

        err = stderr.ToString();
        return stdout.ToString();
    }

    private static DisposableRuntime MakeForTests(StringBuilder stdout, StringBuilder stderr) => new(stdout, stderr);
}
