using System.Collections.Generic;
using Delta.Slang.Syntax;

namespace Delta.Slang.Symbols;

internal static class BuiltinTypes
{
    public static TypeSymbol Invalid { get; } = new TypeSymbol("?", null);
    public static TypeSymbol Int { get; } = new TypeSymbol("int", 0);
    public static TypeSymbol Double { get; } = new TypeSymbol("double", 0.0);
    public static TypeSymbol Bool { get; } = new TypeSymbol("bool", false);
    public static TypeSymbol String { get; } = new TypeSymbol("string", string.Empty);
    public static TypeSymbol Void { get; } = new TypeSymbol("void", 0);

    public static IEnumerable<TypeSymbol> All
    {
        get
        {
            yield return Int;
            yield return Double;
            yield return Bool;
            yield return String;
            yield return Void;
        }
    }
}

internal static class BuiltinFunctions
{
    private static readonly List<FunctionSymbol> all;

    static BuiltinFunctions()
    {
        all = new List<FunctionSymbol>();

        static T add<T>(T function) where T : FunctionSymbol
        {
            all.Add(function);
            return function;
        }

        Print = add(new FunctionSymbol("print", new[] { new ParameterSymbol("text", BuiltinTypes.String) }, BuiltinTypes.Void));
        Input = add(new FunctionSymbol("input", System.Array.Empty<ParameterSymbol>(), BuiltinTypes.String));
        Rndi = add(new FunctionSymbol("rndi", new[] { new ParameterSymbol("max", BuiltinTypes.Int) }, BuiltinTypes.Int));
        Rnd = add(new FunctionSymbol("rnd", new[] { new ParameterSymbol("max", BuiltinTypes.Double) }, BuiltinTypes.Double));

        // unary operator-bound functions
        LogicalNegationOperator = add(new UnaryOperatorFunctionSymbol(TokenKind.Exclamation.GetUnaryOperatorDescriptor(), BuiltinTypes.Bool));

        IntUnaryPlusOperator = add(new UnaryOperatorFunctionSymbol(TokenKind.Plus.GetUnaryOperatorDescriptor(), BuiltinTypes.Int));
        IntUnaryMinusOperator = add(new UnaryOperatorFunctionSymbol(TokenKind.Minus.GetUnaryOperatorDescriptor(), BuiltinTypes.Int));

        DoubleUnaryPlusOperator = add(new UnaryOperatorFunctionSymbol(TokenKind.Plus.GetUnaryOperatorDescriptor(), BuiltinTypes.Double));
        DoubleUnaryMinusOperator = add(new UnaryOperatorFunctionSymbol(TokenKind.Minus.GetUnaryOperatorDescriptor(), BuiltinTypes.Double));

        // binary operator-bound functions
        IntAdditionOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Plus.GetBinaryOperatorDescriptor(), BuiltinTypes.Int));
        IntSubtractionOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Minus.GetBinaryOperatorDescriptor(), BuiltinTypes.Int));
        IntMultiplicationOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Star.GetBinaryOperatorDescriptor(), BuiltinTypes.Int));
        IntDivisionOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Slash.GetBinaryOperatorDescriptor(), BuiltinTypes.Int));

        IntEqualityOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.EqualEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Int, BuiltinTypes.Bool));
        IntDifferenceOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.ExclamationEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Int, BuiltinTypes.Bool));
        IntLowerOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Lower.GetBinaryOperatorDescriptor(), BuiltinTypes.Int, BuiltinTypes.Bool));
        IntLowerOrEqualOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.LowerEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Int, BuiltinTypes.Bool));
        IntGreaterOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Greater.GetBinaryOperatorDescriptor(), BuiltinTypes.Int, BuiltinTypes.Bool));
        IntGreaterOrEqualOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.GreaterEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Int, BuiltinTypes.Bool));

        DoubleAdditionOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Plus.GetBinaryOperatorDescriptor(), BuiltinTypes.Double));
        DoubleSubtractionOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Minus.GetBinaryOperatorDescriptor(), BuiltinTypes.Double));
        DoubleMultiplicationOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Star.GetBinaryOperatorDescriptor(), BuiltinTypes.Double));
        DoubleDivisionOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Slash.GetBinaryOperatorDescriptor(), BuiltinTypes.Double));

        DoubleEqualityOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.EqualEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Double, BuiltinTypes.Bool));
        DoubleDifferenceOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.ExclamationEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Double, BuiltinTypes.Bool));
        DoubleLowerOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Lower.GetBinaryOperatorDescriptor(), BuiltinTypes.Double, BuiltinTypes.Bool));
        DoubleLowerOrEqualOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.LowerEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Double, BuiltinTypes.Bool));
        DoubleGreaterOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Greater.GetBinaryOperatorDescriptor(), BuiltinTypes.Double, BuiltinTypes.Bool));
        DoubleGreaterOrEqualOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.GreaterEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Double, BuiltinTypes.Bool));

        BoolEqualityOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.EqualEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Bool));
        BoolDifferenceOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.ExclamationEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.Bool));

        StringConcatenationOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.Plus.GetBinaryOperatorDescriptor(), BuiltinTypes.String));
        StringEqualityOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.EqualEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.String, BuiltinTypes.Bool));
        StringDifferenceOperator = add(new BinaryOperatorFunctionSymbol(TokenKind.ExclamationEqual.GetBinaryOperatorDescriptor(), BuiltinTypes.String, BuiltinTypes.Bool));
    }

    public static IEnumerable<FunctionSymbol> All => all;

    // Standard Lib
    public static FunctionSymbol Print { get; }
    public static FunctionSymbol Input { get; }
    public static FunctionSymbol Rndi { get; }
    public static FunctionSymbol Rnd { get; }

    // Operator-bound functions
    public static UnaryOperatorFunctionSymbol LogicalNegationOperator { get; }
    public static UnaryOperatorFunctionSymbol IntUnaryPlusOperator { get; }
    public static UnaryOperatorFunctionSymbol IntUnaryMinusOperator { get; }
    public static UnaryOperatorFunctionSymbol DoubleUnaryPlusOperator { get; }
    public static UnaryOperatorFunctionSymbol DoubleUnaryMinusOperator { get; }

    public static BinaryOperatorFunctionSymbol IntAdditionOperator { get; }
    public static BinaryOperatorFunctionSymbol IntSubtractionOperator { get; }
    public static BinaryOperatorFunctionSymbol IntMultiplicationOperator { get; }
    public static BinaryOperatorFunctionSymbol IntDivisionOperator { get; }
    public static BinaryOperatorFunctionSymbol IntEqualityOperator { get; }
    public static BinaryOperatorFunctionSymbol IntDifferenceOperator { get; }
    public static BinaryOperatorFunctionSymbol IntLowerOperator { get; }
    public static BinaryOperatorFunctionSymbol IntLowerOrEqualOperator { get; }
    public static BinaryOperatorFunctionSymbol IntGreaterOperator { get; }
    public static BinaryOperatorFunctionSymbol IntGreaterOrEqualOperator { get; }

    public static BinaryOperatorFunctionSymbol DoubleAdditionOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleSubtractionOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleMultiplicationOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleDivisionOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleEqualityOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleDifferenceOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleLowerOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleLowerOrEqualOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleGreaterOperator { get; }
    public static BinaryOperatorFunctionSymbol DoubleGreaterOrEqualOperator { get; }

    public static BinaryOperatorFunctionSymbol BoolEqualityOperator { get; }
    public static BinaryOperatorFunctionSymbol BoolDifferenceOperator { get; }

    public static BinaryOperatorFunctionSymbol StringConcatenationOperator { get; }
    public static BinaryOperatorFunctionSymbol StringEqualityOperator { get; }
    public static BinaryOperatorFunctionSymbol StringDifferenceOperator { get; }
}
