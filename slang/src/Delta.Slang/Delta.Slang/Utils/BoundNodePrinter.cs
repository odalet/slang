using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using Delta.Slang.Semantic;
using Delta.Slang.Symbols;
using Delta.Slang.Syntax;

namespace Delta.Slang.Utils
{
    public static class BoundNodePrinter
    {
        public static void WriteTo(this BoundTreeNode node, TextWriter writer)
        {
            if (writer is IndentedTextWriter iw)
                WriteTo(node, iw);
            else
                WriteTo(node, new IndentedTextWriter(writer));
        }

        private static void WriteTo(this BoundTreeNode node, IndentedTextWriter writer)
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
                case UnaryExpression ue:
                    WriteUnaryExpression(ue, writer);
                    break;
                case BinaryExpression be:
                    WriteBinaryExpression(be, writer);
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

        private static void WriteInvalidExpression(InvalidExpression node, IndentedTextWriter writer)
        {
            if (node.Expression == null)
                writer.WriteInvalid("!!");
            else
            {
                writer.WriteInvalid("!");
                node.Expression.WriteTo(writer);
                writer.WriteInvalid("!");
            }
        }

        private static void WriteReturnStatement(ReturnStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(TokenKind.ReturnKeyword);
            if (node.Expression != null)
            {
                writer.WriteSpace();
                node.Expression.WriteTo(writer);
            }
            writer.WritePunctuation(TokenKind.Semicolon);
            writer.WriteLine();
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
            finally { writer.Indent = indent; }
            writer.WriteLine();
        }
        
        private static void WriteExpressionStatement(ExpressionStatement node, IndentedTextWriter writer)
        {
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(TokenKind.Semicolon);
            writer.WriteLine();
        }

        private static void WriteBlock(Block node, IndentedTextWriter writer)
        {
            writer.WritePunctuation(TokenKind.OpenBrace);
            writer.WriteLine();
            writer.Indent++;

            foreach (var s in node.Statements)
                s.WriteTo(writer);

            writer.Indent--;
            writer.WritePunctuation(TokenKind.CloseBrace);
        }

        private static void WriteVariableDeclaration(VariableDeclaration node, IndentedTextWriter writer)
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
            node.Initializer.WriteTo(writer);
            writer.WritePunctuation(TokenKind.Semicolon);
            writer.WriteLine();
        }

        private static void WriteFunctionDefinition(FunctionDefinition node, IndentedTextWriter writer)
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
            node.Body.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteIfStatement(IfStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(TokenKind.IfKeyword);
            writer.WriteSpace();
            writer.WritePunctuation(TokenKind.OpenParenthesis);
            node.Condition.WriteTo(writer);
            writer.WritePunctuation(TokenKind.CloseParenthesis);

            var isThenBlock = node.Then is Block;
            if (isThenBlock)
                writer.WriteSpace();
            else
            {
                writer.WriteLine();
                writer.Indent++;
            }

            node.Then.WriteTo(writer);

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

            node.Else.WriteTo(writer);

            if (!isElseBlock)
                writer.Indent--;

            // This leaves a blank line between the end of the if/else statement and the next instructions
            writer.WriteLine();
            writer.WriteLine();
        }

        private static void WriteUnaryExpression(UnaryExpression node, IndentedTextWriter writer)
        {
            var precedence = node.Op.TokenKind.GetUnaryOperatorPrecedence();

            writer.WritePunctuation(node.Op.TokenKind);
            writer.WriteNestedExpression(precedence, node.Operand);
        }

        private static void WriteBinaryExpression(BinaryExpression node, IndentedTextWriter writer)
        {
            var precedence = node.Op.TokenKind.GetBinaryOperatorPrecedence();

            writer.WriteNestedExpression(precedence, node.Left);
            writer.WriteSpace();
            writer.WritePunctuation(node.Op.TokenKind);
            writer.WriteSpace();
            writer.WriteNestedExpression(precedence, node.Right);
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

        private static void WriteConversionExpression(ConversionExpression node, IndentedTextWriter writer)
        {
            writer.WriteTypeIdentifier(node.Type.Name);
            writer.WritePunctuation(TokenKind.OpenParenthesis);
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(TokenKind.CloseParenthesis);
        }

        private static void WriteInvokeExpression(InvokeExpression node, IndentedTextWriter writer)
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

                argument.WriteTo(writer);
            }

            writer.WritePunctuation(TokenKind.CloseParenthesis);
        }

        private static void WriteVariableExpression(VariableExpression node, IndentedTextWriter writer) => writer.WriteIdentifier(node.Variable.Name);

        private static void WriteAssignmentExpression(AssignmentExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WritePunctuation(TokenKind.Equal);
            writer.WriteSpace();
            node.Expression.WriteTo(writer);
        }

        private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, Expression expression)
        {
            if (expression is UnaryExpression unary)
                writer.WriteNestedExpression(parentPrecedence, unary.Op.TokenKind.GetUnaryOperatorPrecedence(), unary);
            else if (expression is BinaryExpression binary)
                writer.WriteNestedExpression(parentPrecedence, binary.Op.TokenKind.GetBinaryOperatorPrecedence(), binary);
            else
                expression.WriteTo(writer);
        }

        private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, int currentPrecedence, Expression expression)
        {
            var needsParenthesis = parentPrecedence >= currentPrecedence;
            if (needsParenthesis) writer.WritePunctuation(TokenKind.OpenParenthesis);
            expression.WriteTo(writer);
            if (needsParenthesis) writer.WritePunctuation(TokenKind.CloseParenthesis);
        }
    }
}
