namespace Slang.CodeAnalysis.Syntax
{
    using System;
    using static SyntaxKind;

    public sealed record ParseTree(SyntaxNode Root);

    public abstract record SyntaxNode(SyntaxKind Kind)
    {
        public abstract SyntaxNode[] Children { get; }
        public abstract R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context);
    }

    public abstract record MemberNode(SyntaxKind Kind) : SyntaxNode(Kind);
    public abstract record StatementNode(SyntaxKind Kind) : SyntaxNode(Kind);
    public abstract record ExpressionNode(SyntaxKind Kind) : StatementNode(Kind);

    // Concrete nodes

    public sealed record CompilationUnitNode(StatementNode[] Statements) : SyntaxNode(CompilationUnit)
    {
        public override SyntaxNode[] Children => Statements;
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record InvalidNode(Token Token) : StatementNode(Invalid)
    {
        public override SyntaxNode[] Children => Array.Empty<SyntaxNode>();
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    // Statements

    public sealed record EmptyNode() : StatementNode(EmptyStatement)
    {
        public override SyntaxNode[] Children => Array.Empty<SyntaxNode>();
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record BlockNode(StatementNode[] Statements) : StatementNode(BlockStatement)
    {
        public override SyntaxNode[] Children => Statements;
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record PrintNode(ExpressionNode Argument, bool AppendNewLine) : StatementNode(PrintStatement)
    {
        public override SyntaxNode[] Children => new[] { Argument };
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record VariableDeclarationNode(Token Name, bool IsReadOnly, ExpressionNode? Initializer = null) : StatementNode(VariableDeclaration)
    {
        public override SyntaxNode[] Children => Initializer == null ? Array.Empty<SyntaxNode>() : new[] { Initializer };
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    // Expressions

    public sealed record AssignmentNode(Token LValue, ExpressionNode Expression) : ExpressionNode(AssignmentExpression)
    {
        public override SyntaxNode[] Children => new[] { Expression };
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record UnaryNode(Token Operator, ExpressionNode Operand) : ExpressionNode(UnaryExpression)
    {
        public override SyntaxNode[] Children => new[] { Operand };
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record BinaryNode(ExpressionNode Left, Token Operator, ExpressionNode Right) : ExpressionNode(BinaryExpression)
    {
        public override SyntaxNode[] Children => new[] { Left, Right };
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record GroupingNode(ExpressionNode Content) : ExpressionNode(GroupingExpression)
    {
        public override SyntaxNode[] Children => new[] { Content };
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record VariableNode(Token Name) : ExpressionNode(VariableExpression)
    {
        public override SyntaxNode[] Children => Array.Empty<SyntaxNode>();
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }

    public sealed record LiteralNode(Token Literal) : ExpressionNode(LiteralExpression)
    {
        public override SyntaxNode[] Children => Array.Empty<SyntaxNode>();
        public override R Accept<R, C>(ISyntaxVisitor<R, C> visitor, C context) => visitor.Visit(this, context);
    }
}
