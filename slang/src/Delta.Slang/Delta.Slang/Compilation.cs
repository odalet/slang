using System;
using System.IO;
using System.Linq;
using Delta.Slang.Semantic;
using Delta.Slang.Syntax;
using Delta.Slang.Utils;

namespace Delta.Slang
{
    public sealed class Compilation
    {
        public Compilation(ParseTree parseTree) => ParseTree = parseTree ?? throw new ArgumentNullException(nameof(parseTree));

        public ParseTree ParseTree { get; }

        //public BoundTree 

        public void EmitTree(TextWriter writer)
        {
            var tree = Binder.BindCompilationUnit(ParseTree.Root);
            if (tree.Statements.Any())
            {
                foreach (var statement in tree.Statements)
                    statement.WriteTo(writer);
            }

            if (tree.Functions.Any())
            {
                foreach (var function in tree.Functions)
                    function.WriteTo(writer);
            }
        }
    }
}
