using System;
using System.Collections.Generic;
using System.Text;

namespace Delta.Slang.Semantics
{
    public interface IHasChildStatements
    {
        IEnumerable<Statement> Statements { get; }
    }

    public interface IHasScope
    {
        Scope Scope { get; }
    }
}
