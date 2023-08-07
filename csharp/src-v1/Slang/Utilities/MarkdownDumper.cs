using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Slang.CodeAnalysis;
using Slang.CodeAnalysis.Syntax;

namespace Slang.Utilities;

public static class MakdownDumperExtensions
{
    public static void DumpTo(this MarkdownDumper dumper, string filename) => File.WriteAllText(filename, dumper.Dump());
}

public sealed class MarkdownDumper
{
    private readonly Token[] tokens;
    private readonly ParseTree tree;
    private readonly IReadOnlyList<IDiagnostic> diagnostics;

    public MarkdownDumper(IEnumerable<Token> lexedTokens, ParseTree parseTree, IReadOnlyList<IDiagnostic> diagnosticCollection)
    {
        tokens = lexedTokens.ToArray();
        tree = parseTree;
        diagnostics = diagnosticCollection;
    }

    public string Dump()
    {
        var builder = new StringBuilder()
            .AppendLine("# Slang Dump")
            .AppendLine()
            ;

        _ = builder
            .AppendLine("## Parse Tree")
            .AppendLine()
            ;

        DumpParseTree(builder);
        _ = builder.AppendLine();

        _ = builder.AppendLine("## Diagnostics")
            .AppendLine()
            ;

        DumpDiagnostics(builder);
        _ = builder.AppendLine();

        _ = builder
            .AppendLine("## Lexed Tokens")
            .AppendLine()
            ;

        DumpTokens(builder);

        return builder.ToString();
    }

    private void DumpDiagnostics(StringBuilder builder)
    {
        if (!diagnostics.Any())
            _ = builder.AppendLine("*None.*");
        else foreach (var diagnostic in diagnostics)
                _ = builder.AppendLine("* " + diagnostic);
    }

    private void DumpTokens(StringBuilder builder)
    {
        if (tokens.Length == 0)
            _ = builder.AppendLine("*None.*");
        else foreach (var token in tokens)
                _ = builder.AppendLine("* " + token);
    }

    private void DumpParseTree(StringBuilder builder)
    {
        var mermaid = new ParseTreeToMermaid(tree);

        _ = builder
            .AppendLine("```mermaid")
            .Append(mermaid.Execute())
            .AppendLine("```")
            ;
    }
}
