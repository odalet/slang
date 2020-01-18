using System;
using System.Collections.Generic;
using System.Linq;
using Delta.Slang.Symbols;

namespace Delta.Slang.Semantic
{
    public enum ScopeKind
    {
        Invalid,
        Global,
        Function,
        Block        
    }

    public sealed class Scope
    {
        private readonly SymbolTable symbols = new SymbolTable();

        public Scope(ScopeKind kind, Scope parent = null)
        {
            Kind = kind;
            Parent = parent;
        }

        public ScopeKind Kind { get; }
        public Scope Parent { get; }

        public bool TryDeclareVariable(VariableSymbol variable) => symbols.TryAdd(variable);
        public bool TryDeclareFunction(FunctionSymbol function) => symbols.TryAdd(function);
        public bool TryDeclareLabel(LabelSymbol label) => symbols.TryAdd(label);

        public bool TryLookupVariable(string name, out VariableSymbol variable) => TryLookupSymbol(SymbolKey.FromVariable(name), out variable);

        ////public bool TryLookupFunction(string name, out FunctionSymbol function) => TryLookupSymbol(name, out function);
        public IEnumerable<FunctionSymbol> LookupFunctions(string name) => LookupFunctions(this, name);

        public bool TryLookupLabel(string name, out LabelSymbol label)
        {
            label = null;
            // We only search for labels inside the current function
            var scope = this;
            var key = SymbolKey.FromLabel(name);
            while (true)
            {
                if (TryLookupSymbol(scope, key, out label, _ => false))
                    return true;

                if (scope.Kind == ScopeKind.Function) // Not found in this function
                    return false;

                scope = scope.Parent;
            }
        }

        public bool TryLookupType(string name, out TypeSymbol type)
        {
            // No custom types for now :)
            type = BuiltinTypes.All.SingleOrDefault(x => x.Name == name);
            return type != null;
        }
        
        public IEnumerable<VariableSymbol> GetDeclaredVariables() => GetDeclaredSymbols<VariableSymbol>();
        public IEnumerable<FunctionSymbol> GetDeclaredFunctions() => GetDeclaredSymbols<FunctionSymbol>();
        public IEnumerable<LabelSymbol> GetDeclaredLabels() => GetDeclaredSymbols<LabelSymbol>();

        private bool TryLookupSymbol<S>(SymbolKey key, out S symbol) where S : Symbol =>
            TryLookupSymbol(this, key, out symbol);

        private IEnumerable<S> GetDeclaredSymbols<S>() where S : Symbol => symbols.All.OfType<S>();

        private static bool TryLookupSymbol<S>(Scope scope, SymbolKey key, out S symbol, Func<Scope, bool> lookupParentCondition = null) where S : Symbol
        {
            symbol = null;

            if (scope.symbols.TryGet(key, out var found))
            {
                if (found is S matchingSymbol)
                {
                    symbol = matchingSymbol;
                    return true;
                }

                return false;
            }
                       
            var parent = scope.Parent;

            var shouldLookupParent = lookupParentCondition == null ? parent != null : lookupParentCondition(parent);
            return shouldLookupParent && TryLookupSymbol(parent, key, out symbol);
        }

        private static IEnumerable<FunctionSymbol> LookupFunctions(Scope scope, string name)
        {
            var found = new Dictionary<SymbolKey, FunctionSymbol>();
            while (scope != null)
            {
                foreach (var symbol in scope.symbols.GetByName(name))
                {
                    if (symbol is FunctionSymbol function)
                    {
                        if (!found.ContainsKey(function.Key))
                            found.Add(function.Key, function);
                    }
                }

                scope = scope.Parent;
            }

            return found.Values;
        }
    }
}
