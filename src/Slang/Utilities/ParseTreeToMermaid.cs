using System.Text;
using Slang.CodeAnalysis.Syntax;

namespace Slang.Utilities
{
    public sealed class ParseTreeToMermaid : BaseSyntaxVisitor<string, ParseTreeToMermaid.Context>
    {
        public enum NodeShape
        {
            Circle,
            Rectangle,
            RoundedRectangle
        }

        public sealed class Context
        {
            private int nodeIndex;

            private readonly StringBuilder declarations = new();
            private readonly StringBuilder graph = new();

            public string Declare(string text, NodeShape shape = NodeShape.Circle)
            {
                nodeIndex++;
                var nodeName = $"node{nodeIndex}";
                _ = declarations.AppendLine($"{nodeName}{OpenNode(shape)}\"{text}\"{CloseNode(shape)}");
                return nodeName;
            }

            public void Wire(string left, string right, string label = "") =>
                graph.AppendLine($"{left} {(string.IsNullOrEmpty(label) ? "---" : $"-- {label} ---")} {right}");

            public override string ToString() => new StringBuilder()
                .AppendLine("flowchart TD")
                .AppendLine()
                .AppendLine(declarations.ToString())
                .AppendLine()
                .AppendLine(graph.ToString())
                .ToString()
                ;

            private static (string open, string close) GetNodeShape(NodeShape shape) => shape switch
            {
                NodeShape.Circle => ("((", "))"),
                NodeShape.Rectangle => ("[", "]"),
                NodeShape.RoundedRectangle => ("(", ")"),
                _ => ("{", "}")
            };

            private static string OpenNode(NodeShape shape) => GetNodeShape(shape).open;
            private static string CloseNode(NodeShape shape) => GetNodeShape(shape).close;
        }

        public ParseTreeToMermaid(ParseTree tree) : base(tree) { }

        public string Execute()
        {
            var context = new Context();
            _ = ParseTree.Root.Accept(this, context);
            return context.ToString();
        }

        public override string Visit(CompilationUnitNode node, Context context)
        {
            var me = context.Declare("Compilation Unit", NodeShape.RoundedRectangle);
            foreach (var statement in node.Statements)
            {
                var child = statement.Accept(this, context);
                context.Wire(me, child);
            }

            return me;
        }

        public override string Visit(EmptyNode node, Context context) =>
            context.Declare("Empty");

        public override string Visit(BlockNode node, Context context)
        {
            var me = context.Declare("{}", NodeShape.RoundedRectangle);
            foreach (var statement in node.Statements)
            {
                var child = statement.Accept(this, context);
                context.Wire(me, child);
            }

            return me;
        }

        public override string Visit(VariableDeclarationNode node, Context context)
        {
            var hasInitializer = node.Initializer != null;
            var decl = node.IsReadOnly ? "val" : "var";
            var text = $"{decl} {node.Name.Text}";
            if (hasInitializer)
                text += " =";

            var me = context.Declare(text, NodeShape.RoundedRectangle);
            if (node.Initializer != null)
            {
                var child = node.Initializer.Accept(this, context);
                context.Wire(me, child);
            }

            return me;
        }

        public override string Visit(PrintNode node, Context context)
        {
            var me = context.Declare("print", NodeShape.RoundedRectangle);
            var child = node.Argument.Accept(this, context);
            context.Wire(me, child);
            return me;
        }

        public override string Visit(IfNode node, Context context)
        {
            var me = context.Declare("if", NodeShape.RoundedRectangle);
            var condition = node.Condition.Accept(this, context);
            context.Wire(me, condition, "?");
            var then = node.Then.Accept(this, context);
            context.Wire(me, then, "then");
            if (node.Else != null)
            {
                var @else = node.Else.Accept(this, context);
                context.Wire(me, @else, "else");
            }

            return me;
        }

        public override string Visit(WhileNode node, Context context)
        {
            var me = context.Declare("while", NodeShape.RoundedRectangle);
            var condition = node.Condition.Accept(this, context);
            context.Wire(me, condition, "?");
            var statement = node.Statement.Accept(this, context);
            context.Wire(me, statement);
            return me;
        }

        public override string Visit(AssignmentNode node, Context context)
        {
            var me = context.Declare(node.LValue.Text);
            var child = node.Expression.Accept(this, context);
            context.Wire(me, child, "=");
            return me;
        }

        public override string Visit(UnaryNode node, Context context)
        {
            var me = context.Declare(node.Operator.Text);
            var child = node.Operand.Accept(this, context);
            context.Wire(me, child);
            return me;
        }

        public override string Visit(BinaryNode node, Context context)
        {
            var me = context.Declare(node.Operator.Text);
            var left = node.Left.Accept(this, context);
            context.Wire(me, left);
            var right = node.Right.Accept(this, context);
            context.Wire(me, right);
            return me;
        }

        public override string Visit(GroupingNode node, Context context)
        {
            var me = context.Declare("()");
            var content = node.Content.Accept(this, context);
            context.Wire(me, content);
            return me;
        }

        public override string Visit(VariableNode node, Context context) => context.Declare(node.Name.Text);

        public override string Visit(LiteralNode node, Context context) =>
            context.Declare(node.Literal.Text);

        public override string Visit(InvalidNode node, Context context) =>
            context.Declare($"Invalid: {node.Token.Text}");

        protected override string VisitFallback(SyntaxNode node, Context context)
        {
            var me = context.Declare(node.GetType().Name, NodeShape.Rectangle);
            foreach (var childNode in node.Children)
            {
                var child = childNode.Accept(this, context);
                context.Wire(me, child);
            }

            return me;
        }
    }
}
