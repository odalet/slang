using System;
using System.Collections.Generic;
using Delta.Slang.Symbols;
using Delta.Slang.Syntax;

namespace Delta.Slang.Semantics;

public abstract class Statement : BoundTreeNode, IHasScope
{
    protected Statement(Scope scope) => Scope = scope ?? throw new ArgumentNullException(nameof(scope));
    public Scope Scope { get; }
}

public sealed class Block : Statement, IHasChildStatements
{
    public Block(Scope scope, IEnumerable<Statement> statements) : base(scope) =>
        Statements = statements ?? Array.Empty<Statement>();

    public IEnumerable<Statement> Statements { get; }
    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.BlockStatement;
}

public sealed class VariableDeclaration : Statement
{
    public VariableDeclaration(Scope scope, VariableSymbol variable, Expression initializer) : base(scope)
    {
        Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
    }

    public VariableSymbol Variable { get; }
    public Expression Initializer { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.VariableDeclaration;
}

public sealed class FunctionDefinition : Statement, IHasChildStatements
{
    public FunctionDefinition(FunctionSymbol declaration, Block body) : base(body.Scope)
    {
        Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }

    public FunctionSymbol Declaration { get; }
    public Block Body { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.FunctionDefinition;

    public IEnumerable<Statement> Statements { get { yield return Body; } }
}

public sealed class ExpressionStatement : Statement
{
    public ExpressionStatement(Scope scope, Expression expression) : base(scope) => Expression = expression ?? throw new ArgumentNullException(nameof(expression));

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.ExpressionStatement;
    public Expression Expression { get; }
}

public sealed class ReturnStatement : Statement
{
    public ReturnStatement(Scope scope, Expression expression) : base(scope) => Expression = expression;

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.ReturnStatement;
    public Expression Expression { get; }
}

public sealed class IfStatement : Statement, IHasChildStatements
{
    public IfStatement(Scope scope, Expression condition, Statement thenStatement, Statement elseStatement) : base(scope)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        Then = thenStatement ?? throw new ArgumentNullException(nameof(thenStatement));
        Else = elseStatement ?? throw new ArgumentNullException(nameof(elseStatement));
    }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.IfStatement;

    public Expression Condition { get; }
    public Statement Then { get; }
    public Statement Else { get; }

    public IEnumerable<Statement> Statements
    {
        get
        {
            yield return Then;
            yield return Else;
        }
    }
}

public sealed class GotoStatement : Statement
{
    internal GotoStatement(Scope scope, GotoStatementNode node, LabelSymbol label, bool isForged = false) : base(scope)
    {
        GotoStatementNode = node ?? throw new ArgumentNullException(nameof(node));
        Label = label;
        IsForged = isForged;
    }

    public LabelSymbol Label { get; private set; }
    public GotoStatementNode GotoStatementNode { get; }
    public bool IsForged { get; }

    public bool IsValid => Label != null;

    public override BoundTreeNodeKind Kind => IsValid ? BoundTreeNodeKind.GotoStatement : BoundTreeNodeKind.InvalidStatement;

    public void Fix(LabelSymbol label) => Label = label ?? throw new ArgumentNullException(nameof(label));
}

public sealed class LabelStatement : Statement
{
    internal LabelStatement(Scope scope, LabelSymbol label, bool isForged = false) : base(scope)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        IsForged = isForged;
    }

    public LabelSymbol Label { get; }
    public bool IsForged { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.LabelStatement;
}
