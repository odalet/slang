using System;
using System.Collections.Generic;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    public abstract class Statement : BoundTreeNode { }

    public sealed class Block : Statement
    {
        public Block(IEnumerable<Statement> statements) =>
            Statements = statements ?? new Statement[0];

        public IEnumerable<Statement> Statements { get; }
        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.BlockStatement;
    }

    public sealed class VariableDeclaration : Statement
    {
        public VariableDeclaration(VariableSymbol variable, Expression initializer)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        public VariableSymbol Variable { get; }
        public Expression Initializer { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.VariableDeclaration;
    }

    public sealed class FunctionDefinition : Statement
    {
        public FunctionDefinition(FunctionSymbol declaration, Statement body)
        {
            Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public FunctionSymbol Declaration { get; }
        public Statement Body { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.FunctionDefinition;
    }

    public sealed class ExpressionStatement : Statement
    {
        public ExpressionStatement(Expression expression) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.ExpressionStatement;
        public Expression Expression { get; }
    }

    public sealed class IfStatement : Statement
    {
        public IfStatement(Expression condition, Statement thenStatement, Statement elseStatement)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Then = thenStatement ?? throw new ArgumentNullException(nameof(thenStatement));
            Else = elseStatement ?? throw new ArgumentNullException(nameof(elseStatement));
        }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.IfStatement;

        public Expression Condition { get; }
        public Statement Then { get; }
        public Statement Else { get; }
    }
}
