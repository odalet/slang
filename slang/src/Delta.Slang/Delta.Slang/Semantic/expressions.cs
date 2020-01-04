using System;
using System.Collections.Generic;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    internal abstract class Expression : BoundTreeNode
    {
        public abstract TypeSymbol Type { get; }
    }

    internal sealed class LiteralExpression : Expression
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
            if (type == BuiltinTypes.Boolean && value is bool) return;
            if (type == BuiltinTypes.Integer && value is int) return;
            if (type == BuiltinTypes.String && value is string) return;
            if (type == BuiltinTypes.Void && value is int i && i == 0) return;

            throw new InvalidOperationException($"Unexpected literal '{value ?? ""}' for type {type.Name}");
        }
    }

    internal sealed class VariableExpression : Expression
    {
        public VariableExpression(VariableSymbol variable) =>
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));

        public override TypeSymbol Type => Variable.Type;
        public VariableSymbol Variable { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.VariableExpression;
    }

    internal sealed class AssignmentExpression : Expression
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

    internal sealed class ConversionExpression : Expression
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

    internal sealed class UnaryExpression : Expression
    {
        public UnaryExpression(UnaryOperator op, Expression operand)
        {
            Op = op ?? throw new ArgumentNullException(nameof(op));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        public override TypeSymbol Type => Op.Type;
        public UnaryOperator Op { get; }
        public Expression Operand { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.UnaryExpression;
    }

    internal sealed class BinaryExpression : Expression
    {
        public BinaryExpression(Expression lhs, BinaryOperator op, Expression rhs)
        {
            Left = lhs ?? throw new ArgumentNullException(nameof(lhs));
            Op = op ?? throw new ArgumentNullException(nameof(op));
            Right = rhs ?? throw new ArgumentNullException(nameof(rhs));
        }

        public override TypeSymbol Type => Op.Type;
        public Expression Left { get; }
        public BinaryOperator Op { get; }
        public Expression Right { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.UnaryExpression;
    }

    internal sealed class InvokeExpression : Expression
    {
        public InvokeExpression(FunctionSymbol function, IEnumerable<Expression> arguments)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));
            Arguments = arguments ?? new Expression[0];
        }

        public override TypeSymbol Type => Function.Type;
        public FunctionSymbol Function { get; }
        public IEnumerable<Expression> Arguments { get; }

        public override BoundTreeNodeKind Kind => BoundTreeNodeKind.InvokeExpression;
    }
}
