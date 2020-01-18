namespace Delta.Slang.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        protected VariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name)
        {
            IsReadOnly = isReadOnly;
            Type = type;
            Key = SymbolKey.FromVariable(name);
        }

        public bool IsReadOnly { get; }
        public TypeSymbol Type { get; }
        public override SymbolKey Key { get; }
    }

    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        internal GlobalVariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name, isReadOnly, type) { }

        public override SymbolKind Kind => SymbolKind.GlobalVariable;
    }

    public class LocalVariableSymbol : VariableSymbol
    {
        internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name, isReadOnly, type) { }

        public override SymbolKind Kind => SymbolKind.LocalVariable;
    }
}
