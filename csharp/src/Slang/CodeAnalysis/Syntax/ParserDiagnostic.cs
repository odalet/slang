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
            MissingVariableInitialization = 3,
            NotAnLValue = 4
        }

        public ParserDiagnostic(string id, DiagnosticSeverity severity, string message, SourceBoundDiagnosticInfo info) : base(id, severity, message, info) { }
    }

    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
    public sealed class ParserException : ApplicationException
    {
        internal ParserException(ParserDiagnostic diagnostic, Token token) : base(diagnostic.Message)
        {
            Diagnostic = diagnostic;
            Token = token;
        }

        public IDiagnostic Diagnostic { get; }
        public Token Token { get; }

        public static ParserException MakeUnexpectedToken(Token token, string? expectation = null)
        {
            var message = $"Unexpected token '{token.SanitizedText}' ({token.Kind})";
            if (!string.IsNullOrEmpty(expectation))
                message += $"; expected: {expectation}";

            var diagnostic = ParserDiagnosticExtensions.MakeParserDiagnostic(UnexpectedToken, token, message);
            return new ParserException(diagnostic, token);
        }

        public static ParserException MakeMissingVariableInitialization(Token token)
        {
            var message = "A read-only variable declared with 'val' must be initialized";
            var diagnostic = ParserDiagnosticExtensions.MakeParserDiagnostic(MissingVariableInitialization, token, message);
            return new ParserException(diagnostic, token);
        }

        public static ParserException MakeNotAnLValue(Token token)
        {
            var message = "Invalid assignment target: not an lvalue";
            var diagnostic = ParserDiagnosticExtensions.MakeParserDiagnostic(NotAnLValue, token, message);
            return new ParserException(diagnostic, token);
        }
    }

    internal static class ParserDiagnosticExtensions
    {
        public static ParserDiagnostic MakeParserDiagnostic(ParserDiagnostic.ErrorCode code, Token token, string message) =>
            new(code.ToId(), DiagnosticSeverity.Error, message, MakeInfo("TODO", token.Position, token.Span));

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
