using System;
using System.Collections.Generic;

namespace Delta.Slang.Syntax
{
    public sealed class FunctionDeclarationNode : MemberNode
    {
        internal FunctionDeclarationNode(Token functionName, ParametersDeclarationNode parameters, TypeClauseNode returnType, BlockNode body)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            ParametersDeclaration = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;
        public override Token MainToken => FunctionName;
        public override IEnumerable<SyntaxNode> Children
        {
            get
            {
                yield return ParametersDeclaration;
                yield return ReturnType;
                yield return Body;
            }
        }

        public Token FunctionName { get; }
        public ParametersDeclarationNode ParametersDeclaration { get; }
        public TypeClauseNode ReturnType { get; }
        public BlockNode Body { get; }
    }
}