using System;
using System.Collections.Generic;

namespace Delta.Slang.Syntax
{
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
}
