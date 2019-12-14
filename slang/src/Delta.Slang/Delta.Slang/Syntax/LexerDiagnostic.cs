using Delta.Slang.Text;

namespace Delta.Slang.Syntax
{
    internal abstract class LexerDiagnostic : Diagnostic
    {
        protected LexerDiagnostic(DiagnosticSeverity severity, int line, TextSpan span, string message) : base(severity, "LEXER", line, span, message)  { }
    }

    internal class LexerError : LexerDiagnostic
    {
        public LexerError(int line, TextSpan span, string message) : base(DiagnosticSeverity.Error, line, span, message) { }
    }
}
