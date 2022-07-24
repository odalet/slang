using System.Text;
using Slang.CodeAnalysis.Syntax;

namespace Slang.Utilities
{
    public sealed class ParseTreeToMermaid : ISyntaxVisitor<string, ParseTreeToMermaid.Context>
    {
        public sealed class Context
        {
            private int nodeIndex;

            private readonly StringBuilder declarations = new();
            private readonly StringBuilder graph = new();

            public string Declare(string text)
            {
                nodeIndex++;
                var nodeName = $"node{nodeIndex}";
                _ = declarations.AppendLine($"{nodeName}((\"{text}\"))");
                return nodeName;
            }

            public void Wire(string left, string right) =>
                graph.AppendLine($"{left} --> {right}");

            public override string ToString() => new StringBuilder()
                .AppendLine("flowchart TD")
                .AppendLine()
                .AppendLine(declarations.ToString())
                .AppendLine()
                .AppendLine(graph.ToString())
                .ToString()
                ;
        }

        private readonly SyntaxNode root;

        public ParseTreeToMermaid(ParseTree parseTree) => root = parseTree.Root;

        public string Dump()
        {
            var context = new Context();
            _ = root.Accept(this, context);
            return context.ToString();
        }

        public string Visit(UnaryExpressionNode node, Context context)
        {
            var me = context.Declare(node.Operator.Text);
            var operand = node.Operand.Accept(this, context);
            context.Wire(me, operand);
            return me;
        }

        public string Visit(BinaryExpressionNode node, Context context)
        {
            var me = context.Declare(node.Operator.Text);
            var left = node.Left.Accept(this, context);
            var right = node.Right.Accept(this, context);
            context.Wire(me, left);
            context.Wire(me, right);
            return me;
        }

        public string Visit(GroupingExpressionNode node, Context context)
        {
            var me = context.Declare("()");
            var content = node.Content.Accept(this, context);
            context.Wire(me, content);
            return me;
        }

        public string Visit(LiteralExpressionNode node, Context context) => 
            context.Declare(node.Literal.Text);

        public string Visit(InvalidExpressionNode node, Context context) =>
            context.Declare($"Invalid: {node.Token.Text}");
    }
}
