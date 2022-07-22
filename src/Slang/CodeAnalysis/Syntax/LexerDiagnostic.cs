using System;
using Slang.CodeAnalysis.Text;

namespace Slang.CodeAnalysis.Syntax
{
    using static LexerDiagnostic.ErrorCode;

    internal class LexerDiagnostic : Diagnostic
    {
        public enum ErrorCode
        {
            UnexpectedError = 1,
            InvalidCharacter = 2,
            InvalidInteger = 3,
            InvalidFloat = 4,
            UnterminatedString = 5,
            UnterminatedComment = 6,
            UnexpectedEndOfComment = 7
        }

        public LexerDiagnostic(string id, DiagnosticSeverity severity, string message, SourceBoundDiagnosticInfo info) : base(id, severity, message, info) { }
    }

    internal static class LexerDiagnosticExtensions
    {
        public static void ReportLexerException(this IDiagnosticSink sink, Exception ex) =>
            ReportLexerError(sink, UnexpectedError, LinePosition.Zero, TextSpan.Zero, $"Unexpected Error: {ex.Message}.\r\n{ex}");
        public static void ReportInvalidCharacter(this IDiagnosticSink sink, LinePosition position, TextSpan span, char character) =>
            ReportLexerError(sink, InvalidCharacter, position, span, $"Encountered invalid character: '{character}'.");
        public static void ReportInvalidInteger(this IDiagnosticSink sink, LinePosition position, TextSpan span, string text) =>
            ReportLexerError(sink, InvalidInteger, position, span, $"'{text}' is not a valid integer.");
        public static void ReportInvalidFloat(this IDiagnosticSink sink, LinePosition position, TextSpan span, string text) =>
            ReportLexerError(sink, InvalidFloat, position, span, $"'{text}' is not a valid floating-point number.");
        public static void ReportUnterminatedString(this IDiagnosticSink sink, LinePosition position, TextSpan span) =>
            ReportLexerError(sink, UnterminatedString, position, span, $"Unterminated string; Probably missing an end quote.");
        public static void ReportUnterminatedComment(this IDiagnosticSink sink, LinePosition position, TextSpan span) =>
            ReportLexerError(sink, UnterminatedComment, position, span, $"Unterminated comment; probably missing '*/'.");
        public static void ReportUnexpectedEndOfComment(this IDiagnosticSink sink, LinePosition position, TextSpan span) =>
            ReportLexerError(sink, UnexpectedEndOfComment, position, span, $"Unexpected end of C comment: nested C comments are not supported.");

        private static void ReportLexerError(this IDiagnosticSink sink, LexerDiagnostic.ErrorCode code, LinePosition position, TextSpan span, string message) =>
            ReportLexerDiagnostic(sink, code.ToId(), DiagnosticSeverity.Error, message, MakeInfo("TODO", position, span));

        private static void ReportLexerDiagnostic(this IDiagnosticSink sink, string id, DiagnosticSeverity severity, string message, SourceBoundDiagnosticInfo info) =>
             sink.Report(new LexerDiagnostic(id, severity, message, info));

        public static string ToId(this LexerDiagnostic.ErrorCode code) => $"L{code.ToString().PadLeft(4, '0')}";
        private static SourceBoundDiagnosticInfo MakeInfo(string filename, LinePosition position, TextSpan span) => new(filename, position, span);
    }
}
