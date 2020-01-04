using System.Collections.Generic;
using System.Linq;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    internal sealed class Scope
    {
        private readonly SymbolTable symbols = new SymbolTable();

        public Scope(Scope parent = null) => Parent = parent;

        public Scope Parent { get; }

        public bool TryDeclareVariable(VariableSymbol variable) => symbols.TryAdd(variable);
        public bool TryDeclareFunction(FunctionSymbol function) => symbols.TryAdd(function);

        public bool TryLookupVariable(string name, out VariableSymbol variable) => TryLookupSymbol(name, out variable);
        public bool TryLookupFunction(string name, out FunctionSymbol function) => TryLookupSymbol(name, out function);
        public bool TryLookupType(string name, out TypeSymbol type)
        {
            // No custom types for now :)
            type = BuiltinTypes.All.SingleOrDefault(x => x.Name == name);
            return type != null;
        }

        public IEnumerable<VariableSymbol> GetDeclaredVariables() => GetDeclaredSymbols<VariableSymbol>();
        public IEnumerable<FunctionSymbol> GetDeclaredFunctions() => GetDeclaredSymbols<FunctionSymbol>();

        private bool TryLookupSymbol<S>(string name, out S symbol) where S : Symbol
        {
            symbol = null;

            if (symbols.TryGet(name, out var found))
            {
                if (found is S matchingSymbol)
                {
                    symbol = matchingSymbol;
                    return true;
                }

                return false;
            }

            return Parent?.TryLookupSymbol(name, out symbol) ?? false;
        }

        private IEnumerable<S> GetDeclaredSymbols<S>() where S : Symbol => symbols.All.OfType<S>();
    }
}
