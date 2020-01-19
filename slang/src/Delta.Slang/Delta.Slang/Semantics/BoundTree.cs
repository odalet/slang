using System;
using System.Collections.Generic;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantics
{
    public sealed class BoundTree : BoundTreeNode, IHasChildStatements, IHasScope
    {
        public BoundTree(Scope scope, IEnumerable<FunctionDefinition> functions, IEnumerable<VariableSymbol> variables, IEnumerable<Statement> statements, IEnumerable<IDiagnostic> diagnostics)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Functions = functions ?? new FunctionDefinition[0];
            Variables = variables ?? new VariableSymbol[0];
            Statements = statements ?? new Statement[0];
            Diagnostics = diagnostics ?? new IDiagnostic[0];
        }

        public Scope Scope { get; }
        public IEnumerable<FunctionDefinition> Functions { get; }
        public IEnumerable<VariableSymbol> Variables { get; }
        public IEnumerable<Statement> Statements { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.Root;
    }
}
