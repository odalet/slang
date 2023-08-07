using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Linq;
using Delta.Slang.Semantics;
using Delta.Slang.Symbols;
using Delta.Slang.Syntax;

namespace Delta.Slang.Utils;

public static class BoundTreeNodeExtensions
{
    private static readonly BoundNodePrinter printer = new(experimentalMode: true);
    public static void WriteTo(this BoundTreeNode node, TextWriter writer) => printer.WriteTo(node, writer);
}

internal sealed class BoundNodePrinter
{
    // TODO: make this a parameter
    private readonly bool experimental = true;

    public BoundNodePrinter(bool experimentalMode) => experimental = experimentalMode;

    public void WriteTo(BoundTreeNode node, TextWriter writer)
    {
        if (writer is IndentedTextWriter iw)
            WriteTo(node, iw);
        else
            WriteTo(node, new IndentedTextWriter(writer));
    }

    private void WriteTo(BoundTreeNode node, IndentedTextWriter writer)
    {
        switch (node)
        {
            case Block b:
                WriteBlock(b, writer);
                break;
            case VariableDeclaration vd:
                WriteVariableDeclaration(vd, writer);
                break;
            case FunctionDefinition fd:
                WriteFunctionDefinition(fd, writer);
                break;
            case ExpressionStatement es:
                WriteExpressionStatement(es, writer);
                break;
            case GotoStatement gs:
                WriteGotoStatement(gs, writer);
                break;
            case LabelStatement ls:
                WriteLabelStatement(ls, writer);
                break;
            case IfStatement ifs:
                WriteIfStatement(ifs, writer);
                break;
            case ReturnStatement rs:
                WriteReturnStatement(rs, writer);
                break;
            case AssignmentExpression ae:
                WriteAssignmentExpression(ae, writer);
                break;
            case VariableExpression ve:
                WriteVariableExpression(ve, writer);
                break;
            case ConversionExpression ce:
                WriteConversionExpression(ce, writer);
                break;
            case InvokeExpression ie:
                WriteInvokeExpression(ie, writer);
                break;
            case LiteralExpression le:
                WriteLiteralExpression(le, writer);
                break;
            case InvalidExpression inve:
                WriteInvalidExpression(inve, writer);
                break;
            case InvalidStatement _:
                writer.Write("<INV>");
                writer.WriteLine();
                break;
            default:
                writer.Write("<?>");
                break;
        }
    }

    private void WriteInvalidExpression(InvalidExpression node, IndentedTextWriter writer)
    {
        if (node.Expression == null)
            writer.WriteInvalid("!!");
        else
        {
            writer.WriteInvalid("!");
            WriteTo(node.Expression, writer);
            writer.WriteInvalid("!");
        }
    }

    private void WriteReturnStatement(ReturnStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword(TokenKind.ReturnKeyword);
        if (node.Expression != null)
        {
            writer.WriteSpace();
            WriteTo(node.Expression, writer);
        }

        writer.WritePunctuation(TokenKind.Semicolon);
        writer.WriteLine();
    }

    private void WriteExpressionStatement(ExpressionStatement node, IndentedTextWriter writer)
    {
        WriteTo(node.Expression, writer);
        writer.WritePunctuation(TokenKind.Semicolon);
        writer.WriteLine();
    }

    private void WriteBlock(Block node, IndentedTextWriter writer)
    {
        writer.WritePunctuation(TokenKind.OpenBrace);
        writer.WriteLine();
        writer.Indent++;

        foreach (var s in node.Statements)
            WriteTo(s, writer);

        writer.Indent--;
        writer.WritePunctuation(TokenKind.CloseBrace);
    }

    private void WriteVariableDeclaration(VariableDeclaration node, IndentedTextWriter writer)
    {
        writer.WriteKeyword(TokenKind.VarKeyword);
        writer.WriteSpace();
        writer.WriteIdentifier(node.Variable.Name);
        writer.WritePunctuation(TokenKind.Colon);
        writer.WriteSpace();
        writer.WriteTypeIdentifier(node.Variable.Type.Name);
        writer.WriteSpace();
        writer.WritePunctuation(TokenKind.Equal);
        writer.WriteSpace();
        WriteTo(node.Initializer, writer);
        writer.WritePunctuation(TokenKind.Semicolon);
        writer.WriteLine();
    }

    private void WriteFunctionDefinition(FunctionDefinition node, IndentedTextWriter writer)
    {
        writer.WriteLine();
        writer.WriteKeyword(TokenKind.FunKeyword);
        writer.WriteSpace();
        writer.WriteIdentifier(node.Declaration.Name);
        writer.WritePunctuation(TokenKind.OpenParenthesis);
        // parameters
        var first = true;
        foreach (var p in node.Declaration.Parameters)
        {
            if (first) first = false;
            else
            {
                writer.WritePunctuation(TokenKind.Comma);
                writer.WriteSpace();
            }

            writer.WriteIdentifier(p.Name);
            writer.WritePunctuation(TokenKind.Colon);
            writer.WriteTypeIdentifier(p.Type.Name);
        }

        writer.WritePunctuation(TokenKind.CloseParenthesis);
        writer.WritePunctuation(TokenKind.Colon);
        writer.WriteSpace();
        writer.WriteTypeIdentifier(node.Declaration.Type.Name);
        writer.WriteSpace();
        WriteTo(node.Body, writer);
        writer.WriteLine();
    }

    private void WriteIfStatement(IfStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword(TokenKind.IfKeyword);
        writer.WriteSpace();
        writer.WritePunctuation(TokenKind.OpenParenthesis);
        WriteTo(node.Condition, writer);
        writer.WritePunctuation(TokenKind.CloseParenthesis);

        var isThenBlock = node.Then is Block;
        if (isThenBlock)
            writer.WriteSpace();
        else
        {
            writer.WriteLine();
            writer.Indent++;
        }

        WriteTo(node.Then, writer);

        if (!isThenBlock)
            writer.Indent--;

        if (node.Else == null)
        {
            writer.WriteLine();
            return;
        }

        if (isThenBlock) writer.WriteSpace();
        writer.WriteKeyword(TokenKind.ElseKeyword);

        var isElseBlock = node.Else is Block;
        if (isElseBlock)
            writer.WriteSpace();
        else
        {
            writer.WriteLine();
            writer.WriteLine();
            writer.Indent++;
        }

        WriteTo(node.Else, writer);

        if (!isElseBlock)
            writer.Indent--;

        // This leaves a blank line between the end of the if/else statement and the next instructions
        writer.WriteLine();
        writer.WriteLine();
    }

    private void WriteConversionExpression(ConversionExpression node, IndentedTextWriter writer)
    {
        writer.WriteTypeIdentifier(node.Type.Name);
        writer.WritePunctuation(TokenKind.OpenParenthesis);
        WriteTo(node.Expression, writer);
        writer.WritePunctuation(TokenKind.CloseParenthesis);
    }

    private void WriteInvokeExpression(InvokeExpression node, IndentedTextWriter writer)
    {
        if (node.Function is UnaryOperatorFunctionSymbol)
            WriteUnaryOperatorFunctionInvocationExpression(node, writer);
        else if (node.Function is BinaryOperatorFunctionSymbol)
            WriteBinaryOperatorFunctionInvocationExpression(node, writer);
        else WriteFunctionInvocationExpression(node, writer);
    }

    private void WriteUnaryOperatorFunctionInvocationExpression(InvokeExpression node, IndentedTextWriter writer)
    {
        if (experimental)
        {
            var function = (UnaryOperatorFunctionSymbol)node.Function;
            var precedence = function.OperatorDescriptor.Precedence;

            writer.WritePunctuation(function.OperatorDescriptor.Operator);
            var childExpression = node.Arguments.ToArray()[0];
            if (childExpression is InvokeExpression ie && ie.Function is IOperatorFunctionSymbol opfs)
                WriteNestedExpression(writer, precedence, opfs.GetOperatorDescriptor().Precedence, childExpression);
            else
                WriteTo(childExpression, writer);
        }
        else WriteFunctionInvocationExpression(node, writer);
    }

    private void WriteBinaryOperatorFunctionInvocationExpression(InvokeExpression node, IndentedTextWriter writer)
    {
        if (experimental)
        {
            var function = (BinaryOperatorFunctionSymbol)node.Function;
            var precedence = function.OperatorDescriptor.Precedence;

            var args = node.Arguments.ToArray();

            var leftExpression = args[0];
            if (leftExpression is InvokeExpression lie && lie.Function is IOperatorFunctionSymbol lopfs)
                WriteNestedExpression(writer, precedence, lopfs.GetOperatorDescriptor().Precedence, leftExpression);
            else
                WriteTo(leftExpression, writer);

            writer.WriteSpace();
            writer.WritePunctuation(function.OperatorDescriptor.Operator);
            writer.WriteSpace();

            var rightExpression = args[1];
            if (rightExpression is InvokeExpression rie && rie.Function is IOperatorFunctionSymbol ropfs)
                WriteNestedExpression(writer, precedence, ropfs.GetOperatorDescriptor().Precedence, rightExpression);
            else
                WriteTo(rightExpression, writer);
        }
        else WriteFunctionInvocationExpression(node, writer);
    }

    private void WriteFunctionInvocationExpression(InvokeExpression node, IndentedTextWriter writer)
    {
        writer.WriteTypeIdentifier(node.Function.Name);
        writer.WritePunctuation(TokenKind.OpenParenthesis);
        var isFirst = true;
        foreach (var argument in node.Arguments)
        {
            if (isFirst) isFirst = false;
            else
            {
                writer.WritePunctuation(TokenKind.Comma);
                writer.WriteSpace();
            }

            WriteTo(argument, writer);
        }

        writer.WritePunctuation(TokenKind.CloseParenthesis);
    }

    private void WriteAssignmentExpression(AssignmentExpression node, IndentedTextWriter writer)
    {
        writer.WriteIdentifier(node.Variable.Name);
        writer.WriteSpace();
        writer.WritePunctuation(TokenKind.Equal);
        writer.WriteSpace();
        WriteTo(node.Expression, writer);
    }

    private void WriteNestedExpression(IndentedTextWriter writer, int parentPrecedence, int currentPrecedence, Expression expression)
    {
        var needsParenthesis = parentPrecedence >= currentPrecedence;
        if (needsParenthesis) writer.WritePunctuation(TokenKind.OpenParenthesis);
        WriteTo(expression, writer);
        if (needsParenthesis) writer.WritePunctuation(TokenKind.CloseParenthesis);
    }

    private static void WriteGotoStatement(GotoStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword(TokenKind.GotoKeyword);
        writer.WriteSpace();
        writer.WriteIdentifier(node.Label?.Name ?? "<UNDEFINED>");
        writer.WritePunctuation(TokenKind.Semicolon);
        writer.WriteLine();
    }

    private static void WriteLabelStatement(LabelStatement node, IndentedTextWriter writer)
    {
        var indent = writer.Indent;
        writer.Indent = 0;
        try
        {
            writer.WriteIdentifier(node.Label.Name);
            writer.WritePunctuation(TokenKind.Colon);
        }
        finally
        {
            writer.Indent = indent;
        }

        writer.WriteLine();
    }

    private static void WriteLiteralExpression(LiteralExpression node, IndentedTextWriter writer)
    {
        if (node.Value is bool b)
            writer.WriteKeyword(b ? TokenKind.TrueKeyword : TokenKind.FalseKeyword);
        else if (node.Value is int i)
            writer.WriteNumber(i.ToString());
        else if (node.Value is double d)
            writer.WriteNumber(d.ToString(CultureInfo.InvariantCulture));
        else if (node.Value is string s)
            writer.WriteNumber($"\"{s ?? ""}\"");
        else if (node.Type == BuiltinTypes.Void)
        {
            writer.WriteTypeIdentifier(BuiltinTypes.Void.Name);
            writer.WritePunctuation(TokenKind.OpenParenthesis);
            writer.WriteNumber("0");
            writer.WritePunctuation(TokenKind.CloseParenthesis);
        }
        else writer.Write("?");
    }

    private static void WriteVariableExpression(VariableExpression node, IndentedTextWriter writer) => writer.WriteIdentifier(node.Variable.Name);
}
