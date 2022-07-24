using Slang.CodeAnalysis.Syntax;

namespace Slang.Utilities
{
    public readonly record struct ParseTreePrettyPrinterOptions(bool JavaStyleBraces, bool CommentOnClosingBrace)
    {
        public static ParseTreePrettyPrinterOptions Default => CSharp;
        public static ParseTreePrettyPrinterOptions Java { get; } = new ParseTreePrettyPrinterOptions(true, true);
        public static ParseTreePrettyPrinterOptions CSharp { get; } = new ParseTreePrettyPrinterOptions(false, false);
    }

    public sealed class ParseTreePrettyPrinter : ISyntaxVisitor<Unit, ParseTreePrettyPrinter.Context>
    {
        public sealed class Context
        {
            private readonly IndentedStringBuilder builder = new();

            public void Indent() => builder.Indent();
            public void Dedent() => builder.Dedent();
            public void Append(string text) => builder.Append(text);
            public void AppendLine(string text) => builder.AppendLine(text);
            public override string ToString() => builder.ToString();
        }

        private readonly SyntaxNode root;
        private readonly ParseTreePrettyPrinterOptions options;

        public ParseTreePrettyPrinter(ParseTree tree) : this(tree, ParseTreePrettyPrinterOptions.Default) { }
        public ParseTreePrettyPrinter(ParseTree tree, ParseTreePrettyPrinterOptions parseTreePrettyPrinterOptions)
        {
            root = tree.Root;
            options = parseTreePrettyPrinterOptions;
        }

        public string Dump()
        {
            var context = new Context();
            _ = root.Accept(this, context);
            return context.ToString();
        }

        public Unit Visit(UnaryExpressionNode node, Context context)
        {
            var header = $"Unary({node.Operator.Text})";
            OpenBlock(context, header);
            context.Indent();
            context.Append("Operand: ");
            _ = node.Operand.Accept(this, context);
            context.Dedent();
            CloseBlock(context, header);
            return Unit.Value;
        }

        public Unit Visit(BinaryExpressionNode node, Context context)
        {
            var header = $"Binary({node.Operator.Text})";
            OpenBlock(context, header);
            context.Indent();
            context.Append("Left : ");
            _ = node.Left.Accept(this, context);
            context.Append("Right: ");
            _ = node.Right.Accept(this, context);
            context.Dedent();
            CloseBlock(context, header);
            return Unit.Value;
        }

        public Unit Visit(GroupingExpressionNode node, Context context)
        {
            var header = "()";
            OpenBlock(context, header);
            context.Indent();
            context.Append("Content: ");
            _ = node.Content.Accept(this, context);
            context.Dedent();
            CloseBlock(context, header);
            return Unit.Value;
        }

        public Unit Visit(LiteralExpressionNode node, Context context)
        {
            context.AppendLine($"Literal({node.Literal.Text})");
            return Unit.Value;
        }

        public Unit Visit(InvalidExpressionNode node, Context context)
        {
            context.AppendLine($"!!Invalid!!({node.Token.Text})");
            return Unit.Value;
        }

        private void OpenBlock(Context context, string header)
        {
            if (options.JavaStyleBraces)
                context.AppendLine($"{header} {{");
            else
            {
                context.AppendLine(header);
                context.AppendLine("{");
            }
        }

        private void CloseBlock(Context context, string header) => context.AppendLine(options.CommentOnClosingBrace ? $"}} // {header}" : "}");
    }
}
