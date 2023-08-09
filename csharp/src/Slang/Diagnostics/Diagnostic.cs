using Slang.Utils;

namespace Slang.Diagnostics;

public abstract record Diagnostic(DiagnosticCode Code);

public record SyntaxDiagnostic(
    DiagnosticCode Code, 
    TextLocation Location, 
    LinePosition StartLinePosition, 
    LinePosition EndLinePosition) : Diagnostic(Code);
