using System.Collections.Generic;

namespace Slang.CodeAnalysis.Syntax
{
    using static OperatorKind;
    using static SyntaxKind;

    public enum OperatorKind
    {
        Unary,
        Binary
    }

    public abstract record OperatorDescriptor(SyntaxKind Operator, OperatorKind Kind, int Precedence);
    public sealed record UnaryOperatorDescriptor(SyntaxKind Operator, int Precedence) : OperatorDescriptor(Operator, Unary, Precedence);
    public sealed record BinaryOperatorDescriptor(SyntaxKind Operator, int Precedence) : OperatorDescriptor(Operator, Binary, Precedence);

    internal static class Operators
    {
        public const int InvalidPrecedence = -1;

        private static readonly Dictionary<SyntaxKind, OperatorDescriptor> bdescriptors = new();
        private static readonly Dictionary<SyntaxKind, OperatorDescriptor> udescriptors = new();

        static Operators()
        {
            static void regb(SyntaxKind op, int precedence) => bdescriptors.Add(op, new BinaryOperatorDescriptor(op, precedence));
            static void regu(SyntaxKind op, int precedence) => udescriptors.Add(op, new UnaryOperatorDescriptor(op, precedence));

            // NB: here is the precedence table from lower to higher:
            //
            //| Name           | Operators | Associativity | Precedence (in code) |
            //| -------------- | --------- | ------------- | -------------------- |
            //| Equality       | == !=     | Left          | 10                   |
            //| Comparison     | < > <= >= | Left          | 20                   |
            //| Addition       | + -       | Left          | 30                   |
            //| Multiplication | * /       | Left          | 40                   |
            //| Unary          | + - !     | **Right**     | 50                   |

            regb(EqualEqualToken, 10);
            regb(BangEqualToken, 10);
            regb(LessToken, 20);
            regb(GreaterToken, 20);
            regb(LessEqualToken, 20);
            regb(GreaterEqualToken, 20);
            regb(PlusToken, 30);
            regb(MinusToken, 30);
            regb(StarToken, 40);
            regb(SlashToken, 40);
            regu(PlusToken, 50);
            regu(MinusToken, 50);
            regu(BangToken, 50);
        }

        ////public static bool IsBinary(this Token token) => IsBinary(token.Kind);
        ////public static bool IsUnary(this Token token) => IsUnary(token.Kind);
        public static int GetBinaryOperatorPrecedence(this Token token) => GetBinaryOperatorPrecedence(token.Kind);
        public static int GetUnaryOperatorPrecedence(this Token token) => GetUnaryOperatorPrecedence(token.Kind);

        ////private static bool IsBinary(SyntaxKind kind) => bdescriptors.ContainsKey(kind);
        ////private static bool IsUnary(SyntaxKind kind) => udescriptors.ContainsKey(kind);
        private static int GetBinaryOperatorPrecedence(SyntaxKind kind) => bdescriptors.ContainsKey(kind) ? bdescriptors[kind].Precedence : InvalidPrecedence;
        private static int GetUnaryOperatorPrecedence(SyntaxKind kind) => udescriptors.ContainsKey(kind) ? udescriptors[kind].Precedence : InvalidPrecedence;
    }
}
