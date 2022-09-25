namespace Slang.CodeAnalysis.Syntax
{
    public interface ISyntaxVisitor<out R, in C>
    {
        R Visit(CompilationUnitNode node, C context);

        // Statements
        R Visit(EmptyNode node, C context);
        R Visit(BlockNode node, C context);
        R Visit(VariableDeclarationNode node, C context);
        R Visit(PrintNode node, C context);
        R Visit(IfNode node, C context);

        // Expressions
        R Visit(AssignmentNode node, C context);
        R Visit(UnaryNode node, C context);
        R Visit(BinaryNode node, C context);
        R Visit(GroupingNode node, C context);
        R Visit(VariableNode node, C context);
        R Visit(LiteralNode node, C context);
        R Visit(InvalidNode node, C context);
    }

    public abstract class BaseSyntaxVisitor<R, C> : ISyntaxVisitor<R, C>
    {
        protected BaseSyntaxVisitor(ParseTree parseTree) => ParseTree = parseTree;

        protected ParseTree ParseTree { get; }

        public virtual R Visit(CompilationUnitNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(EmptyNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(BlockNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(VariableDeclarationNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(PrintNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(IfNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(AssignmentNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(UnaryNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(BinaryNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(GroupingNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(VariableNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(LiteralNode node, C context) => VisitFallback(node, context);
        public virtual R Visit(InvalidNode node, C context) => VisitFallback(node, context);

        protected abstract R VisitFallback(SyntaxNode node, C context);
    }
}
