using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    public sealed class InvalidStatement : Statement
    {
        public InvalidStatement()
        {
            var foo = 42; // For debugging only
        }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvalidStatement;
    }

    public sealed class InvalidExpression : Expression
    {
        public InvalidExpression()
        {
            var foo = 42; // For debugging only
        }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvalidExpression;
        public override TypeSymbol Type => BuiltinTypes.Invalid;
    }
}
