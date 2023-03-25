using System;
using System.Diagnostics.CodeAnalysis;
using Slang.CodeAnalysis.Syntax;

namespace Slang.Runtime
{
    internal enum JumpKind
    {
        Break,
        Continue
    }

    // This exception is used to handle "jump" instructions (that is break and continue in loops)
    [SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "By Design")]
    internal sealed class JumpException : Exception
    {
        public JumpException(JumpKind kind, Token token)
        {
            Kind = kind;
            Token = token;
        }

        public JumpKind Kind { get; }
        public Token Token { get; }

        public static JumpException Break(Token token) => new(JumpKind.Break, token);
        public static JumpException Continue(Token token) => new(JumpKind.Continue, token);
    }
}
