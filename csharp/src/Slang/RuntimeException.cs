using System;
using System.Runtime.Serialization;
using Slang.CodeAnalysis.Syntax;

namespace Slang;

[Serializable]
public sealed class RuntimeException : ApplicationException
{
    public RuntimeException(string message) : base(message) => Details = "";
    public RuntimeException(string message, Token token) : this(message) 
    {
        Details = $"'{token.SanitizedText}' at {token.Position}";
    }

    private RuntimeException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) => Details = "";

    public string Details { get; }
    public override string Message => string.IsNullOrEmpty(Details) ? base.Message : $"{base.Message} - {Details}";
}
