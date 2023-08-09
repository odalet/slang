using Slang.Utils;

namespace Slang.Syntax;

public sealed record SyntaxToken(
    SyntaxKind Kind, 
    TextLocation Location, 
    LinePosition StartLinePosition, 
    LinePosition EndLinePosition);
