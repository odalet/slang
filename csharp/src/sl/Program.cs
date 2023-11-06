using System;
using System.Diagnostics.CodeAnalysis;
using Slang.Syntax;
// ReSharper disable MemberCanBeMadeStatic.Local

namespace Slang.Cli;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
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

        const string sourceCode = """
                                  + // line comment
                                  {{
                                    /* multi-line
                                    comment */
                                    -
                                    []
                                  }}
                                  
                                  fun foo(i: int): void {
                                      val i = 42;
                                      val j = 0x42;
                                      val k = 0xDeadBeef;
                                      val b = 0b11100010101;
                                      val d = 3.14;
                                      val e = 0.314e-1;
                                      val a1 = 1e10;
                                      val a2 = -1e10;
                                      val a3 = +1e10;
                                      val b1 = 1.e10;
                                      val b2 = -1.e10;
                                      val b3 = +1.e10;
                                      val c1 = 1.0e10;
                                      val c2 = -1.0e10;
                                      val c3 = +1.0e10;
                                      val d1 = 1.0e-10;
                                      val d2 = -1.0e-10;
                                      val d3 = +1.0e-10;
                                      val e1 = 1.0e+10;
                                      val e2 = -1.0e+10;
                                      val e3 = +1.0e+10;
                                      val hello = "Hello, World!\r\n";
                                      while(true) {
                                          if (1 == 2) 
                                              return 42.23;
                                          else
                                              continue;
                                      }
                                      return 0;
                                  }
                                  """;

        var span = sourceCode.AsSpan();
        var tokens = new Lexer(span).Lex();
        
        var prettifier = new TokensPrettifier(span);
        prettifier.Dump(tokens);
        
        Console.WriteLine("Press any key to exit");
        _ = Console.ReadKey();

        return ExitCode.OK;
    }
}