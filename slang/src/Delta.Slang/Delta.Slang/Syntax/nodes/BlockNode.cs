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
}