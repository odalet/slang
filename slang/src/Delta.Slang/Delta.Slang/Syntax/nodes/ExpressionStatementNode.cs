using System;
using System.Collections.Generic;

namespace Delta.Slang.Syntax
{
    public sealed class ExpressionStatementNode : StatementNode
    {
        internal ExpressionStatementNode(ExpressionNode expression) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        public override Token MainToken => null;
        public override IEnumerable<SyntaxNode> Children { get { yield return Expression; } }

        public ExpressionNode Expression { get; }
    }
}