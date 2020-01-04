namespace Delta.Slang.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        internal TypeSymbol(string name, object defaultValue) : base(name) => DefaultValue = defaultValue;

        public override SymbolKind Kind => SymbolKind.Type;

        public object DefaultValue { get; }
    }
}