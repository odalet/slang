namespace Slang.CodeAnalysis.Syntax
{
    public interface ISyntaxVisitor<out R, in C>
    {
        R Visit(UnaryExpressionNode node, C context);
        R Visit(BinaryExpressionNode node, C context);
        R Visit(GroupingExpressionNode node, C context);
        R Visit(LiteralExpressionNode node, C context);
        R Visit(InvalidExpressionNode node, C context);
    }

#if NEED_CONTEXTLESS_VISITOR

    // Visitor with no context
    public interface ISyntaxVisitor<out R> : ISyntaxVisitor<R, Unit> { }

    public abstract class BaseSyntaxVisitor<R> : ISyntaxVisitor<R>
    {
        public abstract R Visit(UnaryExpressionNode node);
        public abstract R Visit(BinaryExpressionNode node);
        public abstract R Visit(GroupingExpressionNode node);
        public abstract R Visit(LiteralExpressionNode node);
        public abstract R Visit(InvalidExpressionNode node);

        public R Visit(UnaryExpressionNode node, Unit _) => Visit(node);
        public R Visit(BinaryExpressionNode node, Unit _) => Visit(node);
        public R Visit(GroupingExpressionNode node, Unit _) => Visit(node);
        public R Visit(LiteralExpressionNode node, Unit _) => Visit(node);
        public R Visit(InvalidExpressionNode node, Unit _) => Visit(node);
    }
#endif
}
