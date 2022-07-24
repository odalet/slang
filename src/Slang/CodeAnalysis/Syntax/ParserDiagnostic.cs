using System;
using System.Diagnostics.CodeAnalysis;
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

    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
    public sealed class ParserException : ApplicationException
    {
        internal ParserException(ParserDiagnostic diagnostic) : base(diagnostic.Message) =>
            Diagnostic = diagnostic;

        public IDiagnostic Diagnostic { get; }

        public static ParserException MakeUnexpectedToken(LinePosition position, TextSpan span, Token actual, string? expectation = null)
        {
            var message = $"Unexpected token '{actual.SanitizedText}' ({actual.Kind})";
            if (!string.IsNullOrEmpty(expectation))
                message += $"; expected: {expectation}";

            var diagnostic = ParserDiagnosticExtensions.MakeParserDiagnostic(UnexpectedToken, position, span, message);
            return new ParserException(diagnostic);
        }
    }

    internal static class ParserDiagnosticExtensions
    {
        public static ParserDiagnostic MakeParserDiagnostic(ParserDiagnostic.ErrorCode code, LinePosition position, TextSpan span, string message) =>
            new(code.ToId(), DiagnosticSeverity.Error, message, MakeInfo("TODO", position, span));

        public static void ReportParserException(this IDiagnosticSink sink, Exception exception)
        {
            var diagnostic = exception is ParserException pex
                ? pex.Diagnostic
                : new ParserDiagnostic(
                    UnexpectedError.ToId(),
                    DiagnosticSeverity.Error,
                    $"Unexpected Error: {exception.Message}.{Environment.NewLine}{exception}",
                    MakeInfo("TODO", LinePosition.Zero, TextSpan.Zero));

            sink.Report(diagnostic);
        }

        private static string ToId(this ParserDiagnostic.ErrorCode code) => $"P{((int)code).ToString().PadLeft(4, '0')}";
        private static SourceBoundDiagnosticInfo MakeInfo(string filename, LinePosition position, TextSpan span) => new(filename, position, span);
    }
}
