using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    internal sealed class InvalidStatement : Statement
    {
        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvalidStatement;
    }

    internal sealed class InvalidExpression : Expression
    {
        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvalidExpression;
        public override TypeSymbol Type => BuiltinTypes.Invalid;
    }
}
