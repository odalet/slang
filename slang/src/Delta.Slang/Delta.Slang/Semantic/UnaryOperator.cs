using System;
using Delta.Slang.Symbols;
using Delta.Slang.Syntax;

namespace Delta.Slang.Semantic
{
    public enum UnaryOperatorKind
    {
        Identity,
        Negation,
        LogicalNegation,
        OnesComplement
    }

    public sealed class UnaryOperator
    {
        public UnaryOperator(TokenKind tokenKind, UnaryOperatorKind kind, TypeSymbol operandType) : this(tokenKind, kind, operandType, operandType) { }
        public UnaryOperator(TokenKind tokenKind, UnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
        {
            TokenKind = tokenKind;
            Kind = kind;
            OperandType = operandType ?? throw new ArgumentNullException(nameof(operandType));
            Type = resultType ?? throw new ArgumentNullException(nameof(resultType));
        }

        public TokenKind TokenKind { get; }
        public UnaryOperatorKind Kind { get; }
        public TypeSymbol OperandType { get; }
        public TypeSymbol Type { get; }
    }

    internal static class UnaryOperatorBinder
    {
        private static readonly UnaryOperator[] operators =
        {
            new UnaryOperator(TokenKind.Exclamation, UnaryOperatorKind.LogicalNegation, BuiltinTypes.Bool),
            new UnaryOperator(TokenKind.Plus, UnaryOperatorKind.Identity, BuiltinTypes.Int),
            new UnaryOperator(TokenKind.Minus, UnaryOperatorKind.Negation, BuiltinTypes.Int),
            //new UnaryOperator(TokenKind.Tilde, BoundUnaryOperatorKind.OnesComplement, TypeSymbol.Int),          
            new UnaryOperator(TokenKind.Plus, UnaryOperatorKind.Identity, BuiltinTypes.Double),
            new UnaryOperator(TokenKind.Minus, UnaryOperatorKind.Negation, BuiltinTypes.Double),
        };

        public static UnaryOperator Bind(TokenKind tokenKind, TypeSymbol operandType)
        {
            foreach (var op in operators)
            {
                if (op.TokenKind == tokenKind && op.OperandType == operandType)
                    return op;
            }

            return null;
        }
    }
}
