using System;
using System.Linq;
using Slang.Syntax;

namespace Slang.Cli;

internal sealed class Program
{
    private enum ExitCode
    {
        // See https://www.freebsd.org/cgi/man.cgi?query=sysexits&apropos=0&sektion=0&manpath=FreeBSD+4.3-RELEASE&format=html
        OK = 0,
        InvalidUsage = 64,
        DataError = 65,
        NoInput = 66
    }

    private static int Main(string[] args) => (int)new Program().Run(args);

    private ExitCode Run(string[] args)
    {
        Console.WriteLine($"Hello Slang: {string.Join(",", args)}");

        ////var sourceCode = "Yo!\0";
        ////var sourceCode = "Yo! ++ -- foo+";
        var sourceCode = @"+ // line comment
{
  /* comment */
  -
}";
        var span = sourceCode.AsSpan();
        var tokens = new Lexer(sourceCode).Lex().ToArray();
        foreach (var token in tokens) 
        {
            //var text = span.Slice(token.Location.Start, token.Location.Length);
            var text = span[token.Location.Start..token.Location.End].ToString()
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .Replace("\v", "\\v")
                .Replace("\f", "\\f")
                ;

            Console.WriteLine($"{token.Kind} -> '{text}' [{token.Location.Start} -> {token.Location.End}]/[{token.StartLinePosition.Line}, {token.StartLinePosition.Column} -> {token.EndLinePosition.Line}, {token.EndLinePosition.Column}]");
        }

        return ExitCode.OK;
    }
}
