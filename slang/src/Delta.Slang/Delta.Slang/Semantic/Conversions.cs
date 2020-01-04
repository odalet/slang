using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    internal sealed class ConversionKind
    {
        public ConversionKind(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;
    }

    internal static class Conversions
    {
        public static readonly ConversionKind None = new ConversionKind(exists: false, isIdentity: false, isImplicit: false);
        public static readonly ConversionKind Identity = new ConversionKind(exists: true, isIdentity: true, isImplicit: true);
        public static readonly ConversionKind Implicit = new ConversionKind(exists: true, isIdentity: false, isImplicit: true);
        public static readonly ConversionKind Explicit = new ConversionKind(exists: true, isIdentity: false, isImplicit: false);

        public static ConversionKind GetConversionKind(TypeSymbol from, TypeSymbol to)
        {
            if (from == to) return Identity;

            if (to == BuiltinTypes.String && (from == BuiltinTypes.Boolean || from == BuiltinTypes.Integer))
                return Explicit;

            if (from == BuiltinTypes.String && (to == BuiltinTypes.Boolean || to == BuiltinTypes.Integer))
                return Explicit;

            return None;
        }
    }
}
