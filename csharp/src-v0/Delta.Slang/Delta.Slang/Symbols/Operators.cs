using System.Collections.Generic;
using Delta.Slang.Syntax;

namespace Delta.Slang.Symbols;

public abstract class OperatorDescriptor
{
    public OperatorDescriptor(TokenKind kind, int precedence)
    {
        Operator = kind;
        Precedence = precedence;
    }

    public TokenKind Operator { get; }
    public int Precedence { get; }
    public abstract string FunctionName { get; }
}

public sealed class UnaryOperatorDescriptor : OperatorDescriptor
{
    public UnaryOperatorDescriptor(TokenKind kind, int precedence) : base(kind, precedence) =>
        FunctionName = $"prefix{kind.GetText()}";

    public override string FunctionName { get; }
}

public sealed class BinaryOperatorDescriptor : OperatorDescriptor
{
    public BinaryOperatorDescriptor(TokenKind kind, int precedence) : base(kind, precedence) =>
        FunctionName = $"infix{kind.GetText()}";

    public override string FunctionName { get; }
}

internal static class Operators
{
    private static readonly Dictionary<TokenKind, UnaryOperatorDescriptor> unaryOperatorDescriptors;
    private static readonly Dictionary<TokenKind, BinaryOperatorDescriptor> binaryOperatorDescriptors;

    static Operators()
    {
        unaryOperatorDescriptors = new Dictionary<TokenKind, UnaryOperatorDescriptor>
        {
            [TokenKind.Plus] = new UnaryOperatorDescriptor(TokenKind.Plus, 60),
            [TokenKind.Minus] = new UnaryOperatorDescriptor(TokenKind.Minus, 60),
            [TokenKind.Exclamation] = new UnaryOperatorDescriptor(TokenKind.Exclamation, 60)
        };

        binaryOperatorDescriptors = new Dictionary<TokenKind, BinaryOperatorDescriptor>
        {
            [TokenKind.Star] = new BinaryOperatorDescriptor(TokenKind.Star, 50),
            [TokenKind.Slash] = new BinaryOperatorDescriptor(TokenKind.Slash, 50),
            [TokenKind.Plus] = new BinaryOperatorDescriptor(TokenKind.Plus, 40),
            [TokenKind.Minus] = new BinaryOperatorDescriptor(TokenKind.Minus, 40),
            [TokenKind.EqualEqual] = new BinaryOperatorDescriptor(TokenKind.EqualEqual, 30),
            [TokenKind.ExclamationEqual] = new BinaryOperatorDescriptor(TokenKind.ExclamationEqual, 30),
            [TokenKind.Lower] = new BinaryOperatorDescriptor(TokenKind.Lower, 30),
            [TokenKind.LowerEqual] = new BinaryOperatorDescriptor(TokenKind.LowerEqual, 30),
            [TokenKind.Greater] = new BinaryOperatorDescriptor(TokenKind.Greater, 30),
            [TokenKind.GreaterEqual] = new BinaryOperatorDescriptor(TokenKind.GreaterEqual, 30)
        };
    }

    public static UnaryOperatorDescriptor GetUnaryOperatorDescriptor(this TokenKind kind) =>
        unaryOperatorDescriptors.ContainsKey(kind) ? unaryOperatorDescriptors[kind] : null;

    public static int GetUnaryOperatorPrecedence(this TokenKind kind) => kind.GetUnaryOperatorDescriptor()?.Precedence ?? 0;

    public static BinaryOperatorDescriptor GetBinaryOperatorDescriptor(this TokenKind kind) =>
        binaryOperatorDescriptors.ContainsKey(kind) ? binaryOperatorDescriptors[kind] : null;

    public static int GetBinaryOperatorPrecedence(this TokenKind kind) => kind.GetBinaryOperatorDescriptor()?.Precedence ?? 0;
}
