using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{

#pragma warning disable S1481 // Unused local variables should be removed
#pragma warning disable CS0219 // Variable is assigned but its value is never used
#pragma warning disable IDE0059 // Unnecessary assignment of a value

    public sealed class InvalidStatement : Statement
    {
        public InvalidStatement() : base(new Scope(ScopeKind.Invalid))
        {
            var foo = 42; // For debugging only
        }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvalidStatement;
    }

#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore CS0219 // Variable is assigned but its value is never used
#pragma warning restore S1481 // Unused local variables should be removed

    public sealed class InvalidExpression : Expression
    {
        public InvalidExpression(Expression nested) =>
            Expression = nested;

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvalidExpression;
        public override TypeSymbol Type => Expression?.Type ?? BuiltinTypes.Invalid;
        public Expression Expression { get; }
    }
}
