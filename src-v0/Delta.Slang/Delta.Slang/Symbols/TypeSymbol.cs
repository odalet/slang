namespace Delta.Slang.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        internal TypeSymbol(string name, object defaultValue) : base(name)
        {
            DefaultValue = defaultValue;
            Key = new SymbolKey(new[] { name });
        }

        public override SymbolKind Kind => SymbolKind.Type;
        public override SymbolKey Key { get; }

        public object DefaultValue { get; }
    }
}