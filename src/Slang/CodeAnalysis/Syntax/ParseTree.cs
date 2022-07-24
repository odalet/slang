namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;

    public sealed record ParseTree(SyntaxNode Root);

    public abstract record SyntaxNode(SyntaxKind Kind)
    {
        public abstract R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context);
    }

    public abstract record MemberNode(SyntaxKind Kind) : SyntaxNode(Kind);
    public abstract record StatementNode(SyntaxKind Kind) : SyntaxNode(Kind);
    public abstract record ExpressionNode(SyntaxKind Kind) : SyntaxNode(Kind);

    public sealed record InvalidExpressionNode(Token Token) : ExpressionNode(Invalid)
    {
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    // Expressions

    public sealed record UnaryExpressionNode(Token Operator, ExpressionNode Operand) : ExpressionNode(UnaryExpression)
    {
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record BinaryExpressionNode(ExpressionNode Left, Token Operator, ExpressionNode Right) : ExpressionNode(BinaryExpression)
    {
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record GroupingExpressionNode(ExpressionNode Content) : ExpressionNode(GroupingExpression)
    {
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record LiteralExpressionNode(Token Literal) : ExpressionNode(LiteralExpression)
    {
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }
}
