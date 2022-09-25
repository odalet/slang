using System;
using System.Collections.Generic;
using System.Xml.Linq;

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
            // NB: here is the precedence table from lower to higher:
            //
            //| Name           | Operators | Associativity | Precedence |
            //| -------------- | --------- | ------------- | ---------- |
            //| Assignment     | =         | Right to Left | 10         |
            //| Logical OR     | \|\|      | Left to Right | 20         |
            //| Logical AND    | &&        | Left to Right | 30         |
            //| Equality       | == !=     | Left to Right | 40         |
            //| Comparison     | < > <= >= | Left to Right | 50         |
            //| Addition       | + -       | Left to Right | 60         |
            //| Multiplication | * /       | Left to Right | 70         |
            //| Unary          | + - !     | Right to Left | 80         |

            var precedence = 0;

            void regb(SyntaxKind op) => bdescriptors.Add(op, new BinaryOperatorDescriptor(op, precedence));
            void regu(SyntaxKind op) => udescriptors.Add(op, new UnaryOperatorDescriptor(op, precedence));

            precedence += 10;
            regb(EqualToken);

            precedence += 10;
            regb(LogicalOrToken);

            precedence += 10;
            regb(LogicalAndToken);

            precedence += 10;
            regb(EqualEqualToken);
            regb(BangEqualToken);

            precedence += 10;
            regb(LessToken);
            regb(GreaterToken);
            regb(LessEqualToken);
            regb(GreaterEqualToken);

            precedence += 10;
            regb(PlusToken);
            regb(MinusToken);

            precedence += 10;
            regb(StarToken);
            regb(SlashToken);

            precedence += 10;
            regu(PlusToken);
            regu(MinusToken);
            regu(BangToken);
        }

        public static int GetBinaryOperatorPrecedence(this Token token) => GetBinaryOperatorPrecedence(token.Kind);
        public static int GetUnaryOperatorPrecedence(this Token token) => GetUnaryOperatorPrecedence(token.Kind);

        private static int GetBinaryOperatorPrecedence(SyntaxKind kind) => bdescriptors.ContainsKey(kind) ? bdescriptors[kind].Precedence : InvalidPrecedence;
        private static int GetUnaryOperatorPrecedence(SyntaxKind kind) => udescriptors.ContainsKey(kind) ? udescriptors[kind].Precedence : InvalidPrecedence;
    }
}
