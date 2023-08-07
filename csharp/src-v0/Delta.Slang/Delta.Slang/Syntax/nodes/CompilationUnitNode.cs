using System;
using System.Collections.Generic;
using System.Linq;

namespace Delta.Slang.Syntax;

public sealed class CompilationUnitNode : SyntaxNode
{
    internal CompilationUnitNode(IEnumerable<MemberNode> members) =>
        Members = members == null ? throw new ArgumentNullException(nameof(members)) : members.ToArray();

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    public override IEnumerable<SyntaxNode> Children => Members;
    public override Token MainToken => null;

    public IEnumerable<MemberNode> Members { get; }
}
