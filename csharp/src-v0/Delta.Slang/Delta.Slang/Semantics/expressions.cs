using System;
using System.Collections.Generic;
using Delta.Slang.Symbols;
using Delta.Slang.Utils;

namespace Delta.Slang.Semantics;

public abstract class Expression : BoundTreeNode
{
    public abstract TypeSymbol Type { get; }
}

public sealed class LiteralExpression : Expression
{
    public LiteralExpression(TypeSymbol type, object value)
    {
        Type = type;
        Value = value;

        EnsureTypeConsistency(type, value);
    }

    public override TypeSymbol Type { get; }
    public object Value { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.LiteralExpression;

    private static void EnsureTypeConsistency(TypeSymbol type, object value)
    {
        if (type == BuiltinTypes.Bool && value is bool) return;
        if (type == BuiltinTypes.Int && value is int) return;
        if (type == BuiltinTypes.Double && value is double) return;
        if (type == BuiltinTypes.String && value is string) return;
        if (type == BuiltinTypes.Void && value is int i && i == 0) return;

        using (CultureUtils.InvariantCulture())
            throw new InvalidOperationException($"Unexpected literal '{value ?? ""}' for type {type.Name}");
    }
}

public sealed class VariableExpression : Expression
{
    public VariableExpression(VariableSymbol variable) =>
        Variable = variable ?? throw new ArgumentNullException(nameof(variable));

    public override TypeSymbol Type => Variable.Type;
    public VariableSymbol Variable { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.VariableExpression;
}

public sealed class AssignmentExpression : Expression
{
    public AssignmentExpression(VariableSymbol variable, Expression expression)
    {
        Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override TypeSymbol Type => Expression.Type;
    public VariableSymbol Variable { get; }
    public Expression Expression { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.AssignmentExpression;
}

public sealed class ConversionExpression : Expression
{
    public ConversionExpression(TypeSymbol type, Expression expression)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override TypeSymbol Type { get; }
    public Expression Expression { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.ConversionExpression;
}

public sealed class InvokeExpression : Expression
{
    public InvokeExpression(FunctionSymbol function, IEnumerable<Expression> arguments)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
        Arguments = arguments ?? Array.Empty<Expression>();
    }

    public override TypeSymbol Type => Function.Type;
    public FunctionSymbol Function { get; }
    public IEnumerable<Expression> Arguments { get; }

    public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvokeExpression;
}
