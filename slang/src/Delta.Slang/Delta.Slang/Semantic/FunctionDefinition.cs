using System;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    public sealed class FunctionDefinition : Statement
    {
        public FunctionDefinition(FunctionSymbol declaration, Statement body)
        {
            Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public FunctionSymbol Declaration { get; }
        public Statement Body { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.FunctionDefinition;
    }
}
