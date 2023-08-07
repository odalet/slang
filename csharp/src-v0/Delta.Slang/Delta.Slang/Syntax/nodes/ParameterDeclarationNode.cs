using System;
using System.Collections.Generic;
using System.Linq;

namespace Delta.Slang.Syntax;

public sealed class ParametersDeclarationNode : SyntaxNode
{
    internal ParametersDeclarationNode(IEnumerable<ParameterDeclarationNode> parameters) => 
        Parameters = parameters == null ? Array.Empty<ParameterDeclarationNode>() : parameters.ToArray();

    public override SyntaxKind Kind => SyntaxKind.ParametersDeclaration;
    public override Token MainToken => null;
    public override IEnumerable<SyntaxNode> Children => Parameters;

    public IEnumerable<ParameterDeclarationNode> Parameters { get; }
}

public sealed class ParameterDeclarationNode : SyntaxNode
{
    internal ParameterDeclarationNode(Token identifier, TypeClauseNode type)
    {
        ParameterName = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public override SyntaxKind Kind => SyntaxKind.ParameterDeclaration;
    public override Token MainToken => ParameterName;
    public override IEnumerable<SyntaxNode> Children { get { yield return Type; } }

    public Token ParameterName { get; }
    public TypeClauseNode Type { get; }
}