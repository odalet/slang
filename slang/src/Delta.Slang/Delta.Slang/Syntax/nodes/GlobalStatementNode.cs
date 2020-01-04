using System;
using System.Collections.Generic;

namespace Delta.Slang.Syntax
{
    public sealed class GlobalStatementNode : MemberNode
    {
        internal GlobalStatementNode(StatementNode statement) => Statement = statement ?? throw new ArgumentNullException(nameof(statement));

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
        public override Token MainToken => Statement.MainToken;
        public override IEnumerable<SyntaxNode> Children { get { yield return Statement; } }

        public StatementNode Statement { get; }
    }
}