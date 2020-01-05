namespace Delta.Slang.Symbols
{
    public sealed class LabelSymbol : Symbol
    {
        internal LabelSymbol(string name) : base(name) { }

        public override SymbolKind Kind => SymbolKind.Label;
    }
}
