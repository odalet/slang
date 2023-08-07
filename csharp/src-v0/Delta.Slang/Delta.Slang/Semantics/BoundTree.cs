using System;
using System.Collections.Generic;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantics;

public sealed class BoundTree : BoundTreeNode, IHasChildStatements, IHasScope
{
    public BoundTree(Scope scope, IEnumerable<FunctionDefinition> functions, IEnumerable<VariableSymbol> variables, IEnumerable<Statement> statements, IEnumerable<IDiagnostic> diagnostics)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Functions = functions ?? Array.Empty<FunctionDefinition>();
        Variables = variables ?? Array.Empty<VariableSymbol>();
        Statements = statements ?? Array.Empty<Statement>();
        Diagnostics = diagnostics ?? Array.Empty<IDiagnostic>();
    }

    public Scope Scope { get; }
    public IEnumerable<FunctionDefinition> Functions { get; }
    public IEnumerable<VariableSymbol> Variables { get; }
    public IEnumerable<Statement> Statements { get; }
    public IEnumerable<IDiagnostic> Diagnostics { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.Root;
}
