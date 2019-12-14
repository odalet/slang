using Delta.Slang.Text;

namespace Delta.Slang
{
    public enum DiagnosticSeverity
    {
        Error,
        Warning
    }

    public interface IDiagnostic
    {
        DiagnosticSeverity Severity { get; }        
        int Line { get; }
        TextSpan Span { get; }
        string Message { get; }
    }

    internal abstract class Diagnostic : IDiagnostic
    {
        protected Diagnostic(DiagnosticSeverity severity, string emitter, int line, TextSpan span, string message)
        {
            Severity = severity;
            Emitter = emitter ?? "?";
            Line = line;
            Span = span;
            Message = message ?? "";
        }

        public DiagnosticSeverity Severity { get; }
        public string Emitter { get; }
        public int Line { get; }
        public TextSpan Span { get; }
        public string Message { get; }

        public override string ToString() => $"{Severity.ToString().ToUpperInvariant()} [{Emitter}] at line {Line}: {Message}";
    }
}
