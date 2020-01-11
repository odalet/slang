using System.Collections.Generic;

namespace Delta.Slang.Symbols
{
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
        public static FunctionSymbol Print { get; } = new FunctionSymbol(
            "print",
            new[] { new ParameterSymbol("text", BuiltinTypes.String) },
            BuiltinTypes.Void);

        public static FunctionSymbol Input { get; } = new FunctionSymbol(
            "input",
            new ParameterSymbol[0],
            BuiltinTypes.String);

        public static FunctionSymbol Rndi { get; } = new FunctionSymbol(
            "rndi",
            new[] { new ParameterSymbol("max", BuiltinTypes.Int) },
            BuiltinTypes.Int);

        public static FunctionSymbol Rnd { get; } = new FunctionSymbol(
            "rnd",
            new[] { new ParameterSymbol("max", BuiltinTypes.Double) },
            BuiltinTypes.Double);

        public static IEnumerable<FunctionSymbol> All
        {
            get
            {
                yield return Print;
                yield return Input;
                yield return Rndi;
            }
        }
    }
}
