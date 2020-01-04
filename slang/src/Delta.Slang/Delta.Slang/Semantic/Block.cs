using System.Collections.Generic;

namespace Delta.Slang.Semantic
{
    public sealed class Block : Statement
    {
        public Block(IEnumerable<Statement> statements) =>
            Statements = statements ?? new Statement[0];

        public IEnumerable<Statement> Statements { get; }
        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.BlockStatement;
    }
}
