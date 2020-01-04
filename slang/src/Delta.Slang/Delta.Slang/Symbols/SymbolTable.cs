using System.Collections.Generic;

namespace Delta.Slang.Symbols
{
    internal sealed class SymbolTable
    {
        private readonly Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();

        public IEnumerable<Symbol> All => symbols.Values;

        public bool Contains(string name) => symbols.ContainsKey(name);
        public void Add(Symbol symbol) => symbols.Add(symbol.Name, symbol);

        public bool TryAdd(Symbol symbol)
        {
            if (Contains(symbol.Name)) return false;
            Add(symbol);
            return true;
        }

        public bool TryGet(string name, out Symbol symbol)
        {
            symbol = null;

            if (!Contains(name)) 
                return false;

            symbol = symbols[name];
            return true;
        }
    }
}
