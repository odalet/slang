using System;

namespace Delta.Slang.Syntax;

public sealed class ParseTree
{
    public ParseTree(CompilationUnitNode compilationUnit) => Root = compilationUnit ?? throw new ArgumentNullException(nameof(compilationUnit));
    public CompilationUnitNode Root { get; }
}
