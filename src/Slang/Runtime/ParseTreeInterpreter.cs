using System;
using Slang.CodeAnalysis.Syntax;

namespace Slang.Runtime
{
    using static SyntaxKind;

    public sealed class ParseTreeInterpreter : BaseSyntaxVisitor<RuntimeValue, ParseTreeInterpreter.Context>
    {
        public sealed class Context
        {
            public override string ToString() => "";
        }

        private static class Check
        {
            public static void NotNull(RuntimeValue value, Token token, string message)
            {
                if (value.IsNull()) throw new RuntimeException(message, token);
            }
        }

        private readonly IRuntimeLib rt;
        private Env env;

        public ParseTreeInterpreter(ParseTree tree, IRuntimeLib runtimeLibrary, Env? environment = null) : base(tree)
        {
            rt = runtimeLibrary;
            env = environment ?? new();
        }

        public int Execute()
        {
            var context = new Context();
            _ = ParseTree.Root.Accept(this, context);
            return 0;
        }

        public override RuntimeValue Visit(CompilationUnitNode node, Context context)
        {
            foreach (var statement in node.Statements)
                _ = statement.Accept(this, context);

            return RuntimeValue.Null;
        }

        public override RuntimeValue Visit(EmptyNode node, Context context) => RuntimeValue.Null;

        public override RuntimeValue Visit(IfNode node, Context context)
        {
            var condition = Evaluate(node.Condition, context);
            if (!condition.IsBool(out var ok))
                throw new RuntimeException("An 'if' condition must evaluate to a boolean value");

            if (ok.Value) return node.Then.Accept(this, context);

            if (node.Else != null)
                return node.Else.Accept(this, context);

            return RuntimeValue.Null;
        }

        public override RuntimeValue Visit(BlockNode node, Context context)
        {
            var savedEnv = env;
            try
            {
                env = new Env(env);
                foreach (var statement in node.Statements)
                    _ = statement.Accept(this, context);
            }
            finally { env = savedEnv; }

            return RuntimeValue.Null;
        }

        public override RuntimeValue Visit(VariableDeclarationNode node, Context context)
        {
            var value = node.Initializer == null ? RuntimeValue.Null : Evaluate(node.Initializer, context);
            env.Declare(node.Name.Text, value, node.IsReadOnly);
            return RuntimeValue.Null; // Maybe we could return the real variable value here; would make the variable declaration an expresison...
        }

        public override RuntimeValue Visit(PrintNode node, Context context)
        {
            var value = Evaluate(node.Argument, context);
            rt.Print(value, node.AppendNewLine);
            return RuntimeValue.Null;
        }

        public override RuntimeValue Visit(AssignmentNode node, Context context)
        {
            var rhs = Evaluate(node.Expression, context);
            env.Set(node.LValue.Text, rhs);
            return rhs;
        }

        public override RuntimeValue Visit(UnaryNode node, Context context)
        {
            var operand = Evaluate(node.Operand, context);
            Check.NotNull(operand, node.Operator, "Operand must not be null");

            if (node.Operator.Kind == BangToken)
            {
                return operand.IsBool(out var boolean)
                    ? new RuntimeValue(!boolean)
                    : throw new RuntimeException("Operand must be a boolean", node.Operator);
            }

            if (!operand.IsDouble(out var value))
                throw new RuntimeException("Operand must be a number", node.Operator);

            if (node.Operator.Kind == PlusToken)
                return new RuntimeValue(value);

            if (node.Operator.Kind == MinusToken)
                return new RuntimeValue(-value);

            throw new RuntimeException("Invalid Unary Operator", node.Operator);
        }

        public override RuntimeValue Visit(BinaryNode node, Context context)
        {
            var lhs = Evaluate(node.Left, context);
            Check.NotNull(lhs, node.Operator, "Operand must not be null");

            var rhs = Evaluate(node.Right, context);
            Check.NotNull(rhs, node.Operator, "Operand must not be null");

            if (node.Operator.Kind is PlusToken && (lhs.IsString() || rhs.IsString()))
                return new RuntimeValue($"{lhs}{rhs}");

            if (node.Operator.Kind is EqualEqualToken)
            {
                if (lhs.IsBool(out var lb) && rhs.IsBool(out var rb))
                    return new RuntimeValue(lb == rb);

                if (lhs.IsString(out var ls) && rhs.IsString(out var rs))
                    return new RuntimeValue(ls == rs);
            }

            if (node.Operator.Kind is BangEqualToken)
            {
                if (lhs.IsBool(out var lb) && rhs.IsBool(out var rb))
                    return new RuntimeValue(lb != rb);

                if (lhs.IsString(out var ls) && rhs.IsString(out var rs))
                    return new RuntimeValue(ls != rs);
            }

            if (!lhs.IsDouble(out var l)) throw new RuntimeException("Left Operand must be a number", node.Operator);
            if (!rhs.IsDouble(out var r)) throw new RuntimeException("Right Operand must be a number", node.Operator);

            return new RuntimeValue(node.Operator.Kind switch
            {
                PlusToken => l + r,
                MinusToken => l - r,
                StarToken => l * r,
                SlashToken => l / r,
                GreaterToken => l > r,
                GreaterEqualToken => l >= r,
                LessToken => l < r,
                LessEqualToken => l <= r,
                EqualEqualToken => l == r,
                BangEqualToken => l != r,
                _ => throw new RuntimeException("Invalid Binary Operator", node.Operator)
            });
        }

        public override RuntimeValue Visit(GroupingNode node, Context context) => Evaluate(node.Content, context);

        public override RuntimeValue Visit(VariableNode node, Context context) => env.Get(node.Name.Text);

        public override RuntimeValue Visit(LiteralNode node, Context context) => new(node.Literal.Value);

        protected override RuntimeValue VisitFallback(SyntaxNode node, Context context) =>
            throw new RuntimeException($"Unexpected Instruction: {node.GetType().Name}");

        private RuntimeValue Evaluate(ExpressionNode node, Context context) => node.Accept(this, context);
    }
}
