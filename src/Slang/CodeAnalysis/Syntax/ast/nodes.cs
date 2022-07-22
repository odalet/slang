namespace Slang.CodeAnalysis.Syntax
{
    using static SyntaxKind;

    public abstract record SyntaxNode(SyntaxKind Kind);

    public abstract record MemberNode(SyntaxKind Kind) : SyntaxNode(Kind);
    public abstract record StatementNode(SyntaxKind Kind) : SyntaxNode(Kind);
    public abstract record ExpressionNode(SyntaxKind Kind) : SyntaxNode(Kind);

    public sealed record InvalidExpressionNode(Token Token) : ExpressionNode(Invalid);

    // Expressions

    public sealed record UnaryExpressionNode(Token Operator, ExpressionNode Operand) : ExpressionNode(UnaryExpression);
    public sealed record BinaryExpressionNode(ExpressionNode Left, Token Operator, ExpressionNode Right) : ExpressionNode(BinaryExpression);
    public sealed record LiteralExpressionNode(Token Literal) : ExpressionNode(LiteralExpression);
}
