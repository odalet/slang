using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Slang.CodeAnalysis.Text;

namespace Slang.CodeAnalysis
{
    public enum DiagnosticSeverity
    {
        Error,
        Warning,
        Information
    }

    public interface IDiagnostic
    {
        string Id { get; }
        DiagnosticSeverity Severity { get; }
        string Message { get; }
        SourceBoundDiagnosticInfo? SourceInfo { get; }
    }

    public sealed record SourceBoundDiagnosticInfo(string FileName, LinePosition Position, TextSpan Span);

    internal abstract class Diagnostic : IDiagnostic
    {
        protected Diagnostic(string id, DiagnosticSeverity severity, string message, SourceBoundDiagnosticInfo? info = null)
        {
            Id = id;
            Severity = severity;
            Message = message;
            SourceInfo = info;
        }

        public string Id { get; }
        public DiagnosticSeverity Severity { get; }
        public string Message { get; }
        public SourceBoundDiagnosticInfo? SourceInfo { get; }

        public override string ToString()
        {
            var message = $"[{Severity.GetAbbreviation()}] {Id}: {Message}";
            if (SourceInfo != null)
            {
                var filename = Path.GetFileName(SourceInfo.FileName);
                message += $"{Environment.NewLine}\tin {filename} at {SourceInfo.Position}";
            }

            return message;
        }
    }

    public interface IDiagnosticSink
    {
        bool HasErrors { get; }
        void Report(IDiagnostic diagnostic);
    }

    public sealed class DiagnosticCollection : IDiagnosticSink, IReadOnlyList<IDiagnostic>
    {
        private readonly List<IDiagnostic> diagnostics = new();

        public int Count => diagnostics.Count;
        public IDiagnostic this[int index] => diagnostics[index];
        public bool HasErrors => diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);


        public void Report(IDiagnostic diagnostic) => diagnostics.Add(diagnostic);

        public IEnumerator<IDiagnostic> GetEnumerator() => diagnostics.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal static class DiagnosticExtensions
    {
        public static string GetAbbreviation(this DiagnosticSeverity severity) => severity switch
        {
            DiagnosticSeverity.Error => "E",
            DiagnosticSeverity.Warning => "W",
            DiagnosticSeverity.Information => "I",
            _ => "?"
        };
    }
}
