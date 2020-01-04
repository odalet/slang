using System.Collections.Generic;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    public sealed class BoundTree
    {
        public BoundTree(IEnumerable<FunctionDefinition> functions, IEnumerable<VariableSymbol> variables, IEnumerable<Statement> statements, IEnumerable<IDiagnostic> diagnostics)
        {
            Functions = functions ?? new FunctionDefinition[0];
            Variables = variables ?? new VariableSymbol[0];
            Statements = statements ?? new Statement[0];
            Diagnostics = diagnostics ?? new IDiagnostic[0];
        }

        public IEnumerable<FunctionDefinition> Functions { get; }
        public IEnumerable<VariableSymbol> Variables { get; }
        public IEnumerable<Statement> Statements { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }
    }
}
