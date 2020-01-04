using System;
using System.Collections.Generic;

namespace Delta.Slang.Syntax
{
    public sealed class BlockNode : StatementNode
    {
        internal BlockNode(IEnumerable<StatementNode> statements) => Statements = statements ?? throw new ArgumentNullException(nameof(statements));

        public override SyntaxKind Kind => SyntaxKind.Block;
        public override Token MainToken => null;
        public override IEnumerable<SyntaxNode> Children => Statements;

        public IEnumerable<StatementNode> Statements { get; }
    }

    public sealed class ExpressionStatementNode : StatementNode
    {
        internal ExpressionStatementNode(ExpressionNode expression) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        public override Token MainToken => null;
        public override IEnumerable<SyntaxNode> Children { get { yield return Expression; } }

        public ExpressionNode Expression { get; }
    }

    public sealed class VariableDeclarationNode : StatementNode
    {
        // NB: either type or initializer may be null, but not bith at the same time.

        internal VariableDeclarationNode(Token identifier, TypeClauseNode type, ExpressionNode initializer)
        {
            VariableName = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Type = type; // may be null.
            Initializer = initializer; // may be null.
        }

        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
        public override Token MainToken => VariableName;
        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                if (Type != null) yield return Type;
                if (Initializer != null) yield return Initializer;
            }
        }

        public Token VariableName { get; }
        public TypeClauseNode Type { get; }
        public ExpressionNode Initializer { get; }
    }

    public sealed class ReturnStatementNode : StatementNode
    {
        internal ReturnStatementNode(ExpressionNode expression) => Expression = expression; // may be null

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        public override Token MainToken => null;
        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                if (Expression != null) yield return Expression;
            }
        }

        public ExpressionNode Expression { get; }
    }

    public sealed class IfStatementNode : StatementNode
    {
        internal IfStatementNode(ExpressionNode condition, StatementNode statement, ElseClauseNode elseClause)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
            Else = elseClause; // may be null
        }

        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public override Token MainToken => null;
        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                yield return Condition;
                yield return Statement;
                if (Else != null) yield return Else;
            }
        }

        public ExpressionNode Condition { get; }
        public StatementNode Statement { get; }
        public ElseClauseNode Else { get; }
    }

    public sealed class ElseClauseNode : SyntaxNode
    {
        public ElseClauseNode(StatementNode statement) => Statement = statement ?? throw new ArgumentNullException(nameof(statement));

        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public override Token MainToken => null;
        public override IEnumerable<SyntaxNode> Children { get { yield return Statement; } }

        public StatementNode Statement { get; }
    }
}
