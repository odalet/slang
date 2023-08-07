using System;
using System.Collections.Generic;
using System.Linq;

namespace Delta.Slang.Symbols;

public readonly struct SymbolKey : IEquatable<SymbolKey>
{
    private readonly string key;

    public SymbolKey(string[] parts) => key = "$" + string.Join(",", parts);

    internal static SymbolKey FromLabel(string name) => new(new[] { name });
    internal static SymbolKey FromVariable(string name) => new(new[] { name });
    internal static SymbolKey FromType(string name) => new(new[] { name });
    internal static SymbolKey FromFunction(string name, IEnumerable<TypeSymbol> parameterTypes)
    {
        var parts = new List<string> { name };
        parts.AddRange(parameterTypes.Select(t => t.Key.ToString()));
        return new SymbolKey(parts.ToArray());
    }

    public override bool Equals(object obj) => obj is SymbolKey sk && Equals(sk);
    public bool Equals(SymbolKey other) => key == other.key;
    public override int GetHashCode() => 249886028 + EqualityComparer<string>.Default.GetHashCode(key);

    public static bool operator ==(SymbolKey left, SymbolKey right) => left.Equals(right);
    public static bool operator !=(SymbolKey left, SymbolKey right) => !(left == right);

    public override string ToString() => key;
}
