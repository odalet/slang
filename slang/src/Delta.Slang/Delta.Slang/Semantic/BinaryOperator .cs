using System;
using Delta.Slang.Symbols;
using Delta.Slang.Syntax;

namespace Delta.Slang.Semantic
{
    public enum BinaryOperatorKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        LogicalAnd,
        LogicalOr,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        Equality,
        NonEquality,
        Lower,
        LowerOrEqual,
        Greater,
        GreaterOrEqual,
    }

    public sealed class BinaryOperator
    {
        public BinaryOperator(TokenKind tokenKind, BinaryOperatorKind kind, TypeSymbol operandType) : this(tokenKind, kind, operandType, operandType) { }
        public BinaryOperator(TokenKind tokenKind, BinaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType) : this(tokenKind, kind, operandType, operandType, resultType) { }
        public BinaryOperator(TokenKind tokenKind, BinaryOperatorKind kind, TypeSymbol lhsType, TypeSymbol rhsType, TypeSymbol resultType)
        {
            TokenKind = tokenKind;
            Kind = kind;
            LeftType = lhsType ?? throw new ArgumentNullException(nameof(lhsType));
            RightType = rhsType ?? throw new ArgumentNullException(nameof(rhsType));
            Type = resultType ?? throw new ArgumentNullException(nameof(resultType));
        }

        public TokenKind TokenKind { get; }
        public BinaryOperatorKind Kind { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol Type { get; }
    }

    internal static class BinaryOperatorBinder
    {
        private static readonly BinaryOperator[] operators =
        {
            new BinaryOperator(TokenKind.Plus, BinaryOperatorKind.Addition, BuiltinTypes.Int),
            new BinaryOperator(TokenKind.Minus, BinaryOperatorKind.Subtraction, BuiltinTypes.Int),
            new BinaryOperator(TokenKind.Star, BinaryOperatorKind.Multiplication, BuiltinTypes.Int),
            new BinaryOperator(TokenKind.Slash, BinaryOperatorKind.Division, BuiltinTypes.Int),
            //new BinaryOperator(TokenKind.Ampersand, BinaryOperatorKind.BitwiseAnd, BuiltinTypes.Integer),
            //new BinaryOperator(TokenKind.Pipe, BinaryOperatorKind.BitwiseOr, BuiltinTypes.Integer),
            //new BinaryOperator(TokenKind.Hat, BinaryOperatorKind.BitwiseXor, BuiltinTypes.Integer),
            new BinaryOperator(TokenKind.EqualEqual, BinaryOperatorKind.Equality, BuiltinTypes.Int, BuiltinTypes.Bool),
            new BinaryOperator(TokenKind.ExclamationEqual, BinaryOperatorKind.NonEquality, BuiltinTypes.Int, BuiltinTypes.Bool),
            new BinaryOperator(TokenKind.Lower, BinaryOperatorKind.Lower, BuiltinTypes.Int, BuiltinTypes.Bool),
            new BinaryOperator(TokenKind.LowerEqual, BinaryOperatorKind.LowerOrEqual, BuiltinTypes.Int, BuiltinTypes.Bool),
            new BinaryOperator(TokenKind.Greater, BinaryOperatorKind.Greater, BuiltinTypes.Int, BuiltinTypes.Bool),
            new BinaryOperator(TokenKind.GreaterEqual, BinaryOperatorKind.GreaterOrEqual, BuiltinTypes.Int, BuiltinTypes.Bool),

            //new BinaryOperator(TokenKind.Ampersand, BinaryOperatorKind.BitwiseAnd, BuiltinTypes.Boolean),
            //new BinaryOperator(TokenKind.AmpersandAmpersand, BinaryOperatorKind.LogicalAnd, BuiltinTypes.Boolean),
            //new BinaryOperator(TokenKind.Pipe, BinaryOperatorKind.BitwiseOr, BuiltinTypes.Boolean),
            //new BinaryOperator(TokenKind.PipePipe, BinaryOperatorKind.LogicalOr, BuiltinTypes.Boolean),
            //new BinaryOperator(TokenKind.Hat, BinaryOperatorKind.BitwiseXor, BuiltinTypes.Boolean),
            new BinaryOperator(TokenKind.EqualEqual, BinaryOperatorKind.Equality, BuiltinTypes.Bool),
            new BinaryOperator(TokenKind.ExclamationEqual, BinaryOperatorKind.NonEquality, BuiltinTypes.Bool),

            new BinaryOperator(TokenKind.Plus, BinaryOperatorKind.Addition, BuiltinTypes.String),
            new BinaryOperator(TokenKind.EqualEqual, BinaryOperatorKind.Equality, BuiltinTypes.String, BuiltinTypes.Bool),
            new BinaryOperator(TokenKind.ExclamationEqual, BinaryOperatorKind.NonEquality, BuiltinTypes.String, BuiltinTypes.Bool),
        };

        public static BinaryOperator Bind(TokenKind tokenKind, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (var op in operators)
            {
                if (op.TokenKind == tokenKind && op.LeftType == leftType && op.RightType == rightType)
                    return op;
            }

            return null;
        }
    }
}
