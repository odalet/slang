using System.Collections.Generic;
using System.Linq;

namespace Delta.Slang.Symbols
{
    internal sealed class SymbolTable
    {
        private readonly Dictionary<SymbolKey, Symbol> symbols = new Dictionary<SymbolKey, Symbol>();

        public IEnumerable<Symbol> All => symbols.Values;

        public bool Contains(SymbolKey key) => symbols.ContainsKey(key);

        public void Add(Symbol symbol) => symbols.Add(symbol.Key, symbol);

        public bool TryAdd(Symbol symbol)
        {
            if (Contains(symbol.Key)) return false;
            Add(symbol);
            return true;
        }

        public bool TryGet(SymbolKey key, out Symbol symbol)
        {
            symbol = null;

            if (!Contains(key))
                return false;

            symbol = symbols[key];
            return true;
        }

        public IEnumerable<Symbol> GetByName(string name) => symbols.Values.Where(x => x.Name == name);
    }
}
