using System;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    internal sealed class VariableDeclaration : Statement
    {
        public VariableDeclaration(VariableSymbol variable, Expression initializer)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        public VariableSymbol Variable { get; }
        public Expression Initializer { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.VariableDeclaration;
    }
}
