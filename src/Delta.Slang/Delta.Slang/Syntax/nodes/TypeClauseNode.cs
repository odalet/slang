using System;
using System.Collections.Generic;

namespace Delta.Slang.Syntax
{
    public sealed class TypeClauseNode : SyntaxNode
    {
        internal TypeClauseNode(Token typeToken) => TypeName = typeToken ?? throw new ArgumentNullException(nameof(typeToken));

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public override Token MainToken => TypeName;
        public override IEnumerable<SyntaxNode> Children { get { yield break; } }

        public Token TypeName { get; }
    }
}