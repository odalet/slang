using System;
using System.Collections.Generic;
using System.Text;

namespace Delta.Slang.Semantic
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
