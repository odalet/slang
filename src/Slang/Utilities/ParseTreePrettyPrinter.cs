using System;
using Slang.CodeAnalysis.Syntax;

namespace Slang.Utilities
{
    public readonly record struct ParseTreePrettyPrinterOptions(bool JavaStyleBraces, bool CommentOnClosingBrace)
    {
        public static ParseTreePrettyPrinterOptions Default => CSharp;
        public static ParseTreePrettyPrinterOptions Java { get; } = new ParseTreePrettyPrinterOptions(true, true);
        public static ParseTreePrettyPrinterOptions CSharp { get; } = new ParseTreePrettyPrinterOptions(false, false);
    }

    public sealed class ParseTreePrettyPrinter : BaseSyntaxVisitor<Unit, ParseTreePrettyPrinter.Context>
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

        private sealed record Block : IDisposable
        {
            private readonly ParseTreePrettyPrinter owner;

            public Block(ParseTreePrettyPrinter parent, Context context, string header, bool useSquareBrackets)
            {
                owner = parent;
                Context = context;
                Header = header;
                SquareBrackets = useSquareBrackets;

                OpenBlock();
                Context.Indent();
            }

            private Context Context { get; }
            private string Header { get; }
            private bool SquareBrackets { get; }

            public void Dispose()
            {
                Context.Dedent();
                CloseBlock();
            }

            private void OpenBlock()
            {
                var brackets = SquareBrackets ? "[" : "{";

                if (owner.options.JavaStyleBraces)
                    Context.AppendLine($"{Header} {brackets}");
                else
                {
                    Context.AppendLine(Header);
                    Context.AppendLine(brackets);
                }
            }

            private void CloseBlock()
            {
                var brackets = SquareBrackets ? "]" : "}";
                var comment = owner.options.CommentOnClosingBrace && !SquareBrackets;
                Context.AppendLine(comment ? $"{brackets} // {Header}" : brackets);
            }
        }

        private readonly ParseTreePrettyPrinterOptions options;

        public ParseTreePrettyPrinter(ParseTree tree) : this(tree, ParseTreePrettyPrinterOptions.Default) { }
        public ParseTreePrettyPrinter(ParseTree tree, ParseTreePrettyPrinterOptions parseTreePrettyPrinterOptions) : base(tree) =>
            options = parseTreePrettyPrinterOptions;

        public string Dump()
        {
            var context = new Context();
            _ = ParseTree.Root.Accept(this, context);
            return context.ToString();
        }

        public override Unit Visit(CompilationUnitNode node, Context context)
        {
            using (EnterBlock(context, "CU"))
            using (EnterBlock(context, "Statements:", true))
            {
                foreach (var statement in node.Statements)
                    _ = statement.Accept(this, context);
            }

            return Unit.Value;
        }

        public override Unit Visit(EmptyNode node, Context context)
        {
            context.AppendLine("Empty");
            return Unit.Value;
        }

        public override Unit Visit(VariableDeclarationNode node, Context context)
        {
            var header = $"Declare ({(node.IsReadOnly ? "RO" : "RW")}): {node.Name}";
            if (node.Initializer == null)
                context.AppendLine(header);
            else using (EnterBlock(context, header))
                {
                    context.Append("Initializer: ");
                    _ = node.Initializer.Accept(this, context);
                }

            return Unit.Value;
        }

        public override Unit Visit(PrintNode node, Context context)
        {
            using (EnterBlock(context, "Print()"))
            {
                context.Append("Argument: ");
                _ = node.Argument.Accept(this, context);
            }

            return Unit.Value;
        }

        public override Unit Visit(IfNode node, Context context)
        {
            using (EnterBlock(context, "if"))
            {
                context.Append("Condition: ");
                _ = node.Condition.Accept(this, context);
                context.Append("Then:");
                _ = node.Then.Accept(this, context);
                if (node.Else != null)
                {
                    context.Append("Else:");
                    _ = node.Else.Accept(this, context);
                }
            }

            return Unit.Value;
        }

        public override Unit Visit(WhileNode node, Context context)
        {
            using (EnterBlock(context, "while"))
            {
                context.Append("Condition: ");
                _ = node.Condition.Accept(this, context);
                context.Append("Statement:");
                _ = node.Statement.Accept(this, context);
            }

            return Unit.Value;
        }

        public override Unit Visit(UnaryNode node, Context context)
        {
            using (EnterBlock(context, $"Unary({node.Operator.Text})"))
            {
                context.Append("Operand: ");
                _ = node.Operand.Accept(this, context);
            }

            return Unit.Value;
        }

        public override Unit Visit(BinaryNode node, Context context)
        {
            using (EnterBlock(context, $"Binary({node.Operator.Text})"))
            {

                context.Append("Left : ");
                _ = node.Left.Accept(this, context);
                context.Append("Right: ");
                _ = node.Right.Accept(this, context);
            }

            return Unit.Value;
        }

        public override Unit Visit(GroupingNode node, Context context)
        {
            using (EnterBlock(context, "()"))
            {
                context.Append("Content: ");
                _ = node.Content.Accept(this, context);
            }

            return Unit.Value;
        }

        public override Unit Visit(VariableNode node, Context context)
        {
            context.AppendLine($"Variable({node.Name.Text})");
            return Unit.Value;
        }

        public override Unit Visit(LiteralNode node, Context context)
        {
            context.AppendLine($"Literal({node.Literal.Text})");
            return Unit.Value;
        }

        public override Unit Visit(InvalidNode node, Context context)
        {
            context.AppendLine($"!!Invalid!!({node.Token.Text})");
            return Unit.Value;
        }

        protected override Unit VisitFallback(SyntaxNode node, Context context)
        {
            using (EnterBlock(context, $"<{node.GetType().Name}>"))
            {
                if (node.Children.Length > 0)
                    using (EnterBlock(context, "Children:", true))
                    {
                        foreach (var child in node.Children)
                            _ = child.Accept(this, context);
                    }
            }

            return Unit.Value;
        }

        private IDisposable EnterBlock(Context context, string header, bool useSquareBrackets = false) => 
            new Block(this, context, header, useSquareBrackets);
    }
}
