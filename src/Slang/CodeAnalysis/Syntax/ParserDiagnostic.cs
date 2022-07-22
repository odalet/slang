using System;
using Slang.CodeAnalysis.Text;

namespace Slang.CodeAnalysis.Syntax
{
    using static ParserDiagnostic.ErrorCode;

    internal sealed class ParserDiagnostic : Diagnostic
    {
        public enum ErrorCode
        {
            UnexpectedError = 1,
            UnexpectedToken = 2,
            ////InvalidCharacter = 2,
            ////InvalidInteger = 3,
            ////InvalidFloat = 4,
            ////UnterminatedString = 5,
            ////UnterminatedComment = 6,
            ////UnexpectedEndOfComment = 7
        }

        public ParserDiagnostic(string id, DiagnosticSeverity severity, string message, SourceBoundDiagnosticInfo info) : base(id, severity, message, info) { }
    }

    internal static class ParserDiagnosticExtensions
    {
        public static void ReportParserException(this IDiagnosticSink sink, Exception ex) =>
            ReportParserError(sink, UnexpectedError, LinePosition.Zero, TextSpan.Zero, $"Unexpected Error: {ex.Message}.\r\n{ex}");

        ////public static void ReportInvalidCharacter(this IDiagnosticSink sink, LinePosition position, TextSpan span, char character) =>
        ////    ReportParserError(sink, InvalidCharacter, position, span, $"Encountered invalid character: '{character}'.");
        ////public static void ReportInvalidInteger(this IDiagnosticSink sink, LinePosition position, TextSpan span, string text) =>
        ////    ReportParserError(sink, InvalidInteger, position, span, $"'{text}' is not a valid integer.");
        ////public static void ReportUnterminatedString(this IDiagnosticSink sink, LinePosition position, TextSpan span) =>
        ////    ReportParserError(sink, UnterminatedString, position, span, $"Unterminated string; Probably missing an end quote.");
        ////public static void ReportUnterminatedComment(this IDiagnosticSink sink, LinePosition position, TextSpan span) =>
        ////    ReportParserError(sink, UnterminatedComment, position, span, $"Unterminated comment; probably missing '*/'.");
        ////public static void ReportUnexpectedEndOfComment(this IDiagnosticSink sink, LinePosition position, TextSpan span) =>
        ////    ReportParserError(sink, UnexpectedEndOfComment, position, span, $"Unexpected end of C comment: nested C comments are not supported.");

        public static void ReportUnexpectedToken(this IDiagnosticSink sink, LinePosition position, TextSpan span, Token actual, string? expectation = null)
        {
            var message = $"Unexpected token '{actual.SanitizedText}' ({actual.Kind})";
            if (!string.IsNullOrEmpty(expectation))
                message += $"; expected: {expectation}";

            ReportParserError(sink, UnexpectedToken, position, span, message);
        }

        private static void ReportParserError(this IDiagnosticSink sink, ParserDiagnostic.ErrorCode code, LinePosition position, TextSpan span, string message) =>
            ReportLexerDiagnostic(sink, code.ToId(), DiagnosticSeverity.Error, message, MakeInfo("TODO", position, span));

        private static void ReportLexerDiagnostic(this IDiagnosticSink sink, string id, DiagnosticSeverity severity, string message, SourceBoundDiagnosticInfo info) =>
             sink.Report(new LexerDiagnostic(id, severity, message, info));

        public static string ToId(this ParserDiagnostic.ErrorCode code) => $"P{((int)code).ToString().PadLeft(4, '0')}";
        private static SourceBoundDiagnosticInfo MakeInfo(string filename, LinePosition position, TextSpan span) => new(filename, position, span);
    }
}
