using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    public sealed class InvalidStatement : Statement
    {
        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvalidStatement;
    }

    public sealed class InvalidExpression : Expression
    {
        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvalidExpression;
        public override TypeSymbol Type => BuiltinTypes.Invalid;
    }
}
