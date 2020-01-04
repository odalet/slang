using System;
using System.Collections.Generic;
using Delta.Slang.Syntax;

namespace Delta.Slang.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, IEnumerable<ParameterSymbol> parameters, TypeSymbol type, FunctionDeclarationNode declaration = null) : base(name)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public FunctionDeclarationNode Declaration { get; }
        public IEnumerable<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
    }
}
