using System.Collections.Generic;

namespace Delta.Slang.Syntax
{
    public sealed class ReturnStatementNode : StatementNode
    {
        internal ReturnStatementNode(ExpressionNode expression) => Expression = expression;

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
}