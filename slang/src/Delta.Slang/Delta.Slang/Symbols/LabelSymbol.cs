namespace Delta.Slang.Symbols
{
    public sealed class LabelSymbol : Symbol
    {
        internal LabelSymbol(string name) : base(name) =>
            Key = SymbolKey.FromLabel(name);

        public override SymbolKind Kind => SymbolKind.Label;
        public override SymbolKey Key { get; }
    }
}
