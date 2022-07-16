using System.Collections.Generic;

namespace Delta.Slang.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public abstract Token MainToken { get; } // most meaningful token associated with this node
        public abstract IEnumerable<SyntaxNode> Children { get; }
    }
}
