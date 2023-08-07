using System.Collections.Generic;

namespace Delta.Slang.Semantics;

public interface IHasChildStatements
{
    IEnumerable<Statement> Statements { get; }
}

public interface IHasScope
{
    Scope Scope { get; }
}
