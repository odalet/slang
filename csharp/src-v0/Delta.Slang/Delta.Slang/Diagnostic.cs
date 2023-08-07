using System;
using System.Collections;
using System.Collections.Generic;
using Delta.Slang.Symbols;
using Delta.Slang.Syntax;
using Delta.Slang.Text;

namespace Delta.Slang;

public enum DiagnosticSeverity
{
    Error,
    Warning
}

public interface IDiagnostic
{
    DiagnosticSeverity Severity { get; }
    LinePosition Position { get; }
    TextSpan Span { get; }
    string Message { get; }
}

internal abstract class Diagnostic : IDiagnostic
{
    protected Diagnostic(DiagnosticSeverity severity, string emitter, LinePosition position, TextSpan span, string message)
    {
        Severity = severity;
        Emitter = emitter ?? "?";
        Position = position;
        Span = span;
        Message = message ?? "";
    }

    public DiagnosticSeverity Severity { get; }
    public string Emitter { get; }
    public LinePosition Position { get; }
    public TextSpan Span { get; }
    public string Message { get; }

    public override string ToString()
    {
        static string w(LinePosition position) => $"({position.Line + 1}, {position.Column + 1})";
        return $"{Severity.ToString().ToUpperInvariant()} [{Emitter}] at {w(Position)}: {Message}";
    }
}

internal abstract class LexerDiagnostic : Diagnostic
{
    protected LexerDiagnostic(DiagnosticSeverity severity, LinePosition position, TextSpan span, string message) : base(severity, "LEXER", position, span, message) { }
}

internal abstract class ParserDiagnostic : Diagnostic
{
    protected ParserDiagnostic(DiagnosticSeverity severity, LinePosition position, TextSpan span, string message) : base(severity, "PARSER", position, span, message) { }
}

internal abstract class BinderDiagnostic : Diagnostic
{
    protected BinderDiagnostic(DiagnosticSeverity severity, LinePosition position, TextSpan span, string message) : base(severity, "BINDER", position, span, message) { }
}

internal class LexerError : LexerDiagnostic
{
    public LexerError(LinePosition position, TextSpan span, string message) : base(DiagnosticSeverity.Error, position, span, message) { }
}

internal class ParserError : ParserDiagnostic
{
    public ParserError(LinePosition position, TextSpan span, string message) : base(DiagnosticSeverity.Error, position, span, message) { }
}

internal class BinderError : BinderDiagnostic
{
    public BinderError(LinePosition position, TextSpan span, string message) : base(DiagnosticSeverity.Error, position, span, message) { }
}

internal class DiagnosticCollection : IEnumerable<IDiagnostic>
{
    private readonly List<IDiagnostic> diagnostics = new();

    public IEnumerator<IDiagnostic> GetEnumerator() => diagnostics.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public DiagnosticCollection AddRange(IEnumerable<IDiagnostic> otherDiagnostics)
    {
        diagnostics.AddRange(otherDiagnostics);
        return this;
    }

    public void ReportLexerException(Exception ex) => ReportLexerError(LinePosition.Zero, TextSpan.Zero, $"Unexpected Error: {ex.Message}.\r\n{ex}");
    public void ReportInvalidCharacter(LinePosition position, TextSpan span, char character) =>
        ReportLexerError(position, span, $"Encountered invalid character: '{character}'.");
    public void ReportInvalidNumber(LinePosition position, TextSpan span, string text) =>
        ReportLexerError(position, span, $"'{text}' is not a valid number.");

    public void ReportParserException(Exception ex) => ReportParserError(LinePosition.Zero, TextSpan.Zero, $"Unexpected Error: {ex.Message}.\r\n{ex}");
    public void ReportUnexpectedToken(LinePosition position, TextSpan span, TokenKind expected, TokenKind actual) =>
        ReportParserError(position, span, $"Unexpected token <{actual}>; expected <{expected}>.");
    public void ReportInvalidVariableDeclaration(LinePosition position, TextSpan span, Token variable, string reason) =>
        ReportParserError(position, span, $"Variable '{variable.Text}' is incorrectly declared: {reason ?? "?"}.");

    public void ReportBinderException(Exception ex) => ReportBinderError(LinePosition.Zero, TextSpan.Zero, $"Unexpected Error: {ex.Message}.\r\n{ex}");
    public void ReportExpressionMustHaveValue(Token where) =>
        ReportBinderError(where, "Expression must have a value.");
    public void ReportParameterAlreadyDeclared(Token where, string name) =>
        ReportBinderError(where, $"A parameter named '{name}' already exists.");
    public void ReportSymbolAlreadyDeclared(Token where, string name, string kind) =>
        ReportBinderError(where, $"{kind} '{name}' is already declared.");
    public void ReportUndefinedType(Token where, string name) =>
        ReportBinderError(where, $"Type '{name}' could not be found.");
    public void ReportUndefinedFunction(Token where, string name) =>
        ReportBinderError(where, $"Function '{name}' could not be found.");
    public void ReportAmbiguousFunction(Token where, string name, string[] argumentTypes) =>
        ReportBinderError(where, $"Invocation of Function '{name}' with argument types ({string.Join(", ", argumentTypes)}) is ambiguous.");
    public void ReportUndefinedFunctionWithArguments(Token where, string name, string[] argumentTypes) =>
        ReportBinderError(where, $"No Function '{name}' could be found that is compatible with argument types ({string.Join(", ", argumentTypes)}).");
    public void ReportUndefinedVariable(Token where, string name) =>
        ReportBinderError(where, $"Variable '{name}' could not be found.");
    public void ReportUndefinedLabel(Token where, string name) =>
        ReportBinderError(where, $"Label '{name}' could not be found.");
    public void ReportInvalidAssignmentToReadOnlyVariable(Token where, string name) =>
        ReportBinderError(where, $"Cannot assign a value to read-only variable '{name}'.");
    public void ReportImpossibleConversion(Token where, TypeSymbol from, TypeSymbol to) =>
        ReportBinderError(where, $"Cannot convert from '{from.Name}' to '{to.Name}'.");
    public void ReportImpossibleImplicitConversion(Token where, TypeSymbol from, TypeSymbol to) =>
        ReportBinderError(where, $"Cannot convert from '{from.Name}' to '{to.Name}'. An explicit conversion exists (are you missing a cast?)");
    public void ReportUndefinedUnaryOperator(Token where, string op, TypeSymbol operandType) =>
        ReportBinderError(where, $"Operator '{op}' is not defined for type '{operandType}'.");
    public void ReportUndefinedBinaryOperator(Token where, string op, TypeSymbol lhsType, TypeSymbol rhsType) =>
        ReportBinderError(where, $"Operator '{op}' is not defined for types '{lhsType}' and '{rhsType}'.");
    public void ReportInvalidLabelDeclaration(Token where, string label) =>
        ReportBinderError(where, $"Label '{label}' is invalid. Labels cannot be defined outside a function.");
    public void ReportInvalidReturn(Token where) =>
        ReportBinderError(where, "The 'return' keyword can only be used inside of functions.");
    public void ReportInconsistentReturnType(Token where, FunctionSymbol function, TypeSymbol expectedType, TypeSymbol actualType) =>
        ReportBinderError(where, $"Return expression type '{actualType}' is inconsistent with Function '{function}' return type '{expectedType}'.");
    
    private void ReportLexerError(LinePosition position, TextSpan span, string message) => diagnostics.Add(new LexerError(position, span, message ?? ""));
    private void ReportParserError(LinePosition position, TextSpan span, string message) => diagnostics.Add(new ParserError(position, span, message ?? ""));
    private void ReportBinderError(Token where, string message) => ReportBinderError(where.Position, where.Span, message);
    private void ReportBinderError(LinePosition position, TextSpan span, string message) => diagnostics.Add(new BinderError(position, span, message ?? ""));
}
