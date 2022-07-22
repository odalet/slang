using System.Linq;

namespace NPicol
{
    internal static class RuntimeCommands
    {
        // Syntax is op left right
        // Eg: / 1 5
        public static Status MathCommand(InterpreterData i, string[] args, object? _)
        {
            if (!CheckArity(i, args, 3)) return Status.Error;

            var op = args[0].Trim();
            var left = long.Parse(args[1]);
            var right = long.Parse(args[2]);
            if (right == 0 && (op == "/" || op == "%"))
            {
                i.SetResult($"Division by zero");
                return Status.Error; // Divide by zero
            }

            var result = 0L;
            if (op[0] == '+') result = left + right;
            else if (op[0] == '-') result = left - right;
            else if (op[0] == '*') result = left * right;
            else if (op[0] == '/') result = left / right;
            else if (op[0] == '%') result = left % right;
            else if (op[0] == '>' && op.Length == 1) result = BoolToLong(left > right);
            else if (op[0] == '<' && op.Length == 1) result = BoolToLong(left < right);
            else if (op[0] == '>' && op.Length > 1 && op[1] == '=') result = BoolToLong(left >= right);
            else if (op[0] == '<' && op.Length > 1 && op[1] == '=') result = BoolToLong(left <= right);
            else if (op[0] == '=' && op.Length > 1 && op[1] == '=') result = BoolToLong(left == right);
            else if (op[0] == '!' && op.Length > 1 && op[1] == '=') result = BoolToLong(left != right);

            i.SetResult(result);

            return Status.OK;
        }

        public static Status SetCommand(InterpreterData i, string[] args, object? _)
        {
            if (!CheckArity(i, args, 2, 3)) return Status.Error;

            if (args.Length == 2)
            {
                // set x -> should return the value of x
                var variable = i.GetVariable(args[1]);
                if (variable != null)
                {
                    i.SetResult(variable.Value);
                    return Status.OK;
                }

                // Variable not found
                i.SetResult($"Undefined variable: '{args[1]}'");
                return Status.Error;
            }

            // set x 42 -> affects 42 to x, then returns 42
            _ = i.SetVariable(args[1], args[2]);
            i.SetResult(args[2]);
            return Status.OK;
        }

        public static Status PutsCommand(InterpreterData i, string[] args, object? _)
        {
            if (!CheckArity(i, args, 2)) return Status.Error;
            RuntimeLib.Puts(args[1]);
            return Status.OK;
        }

        private static bool CheckArity(InterpreterData i, string[] args, int expected) => CheckArity(i, args, new[] { expected });
        private static bool CheckArity(InterpreterData i, string[] args, params int[] expected)
        {
            var actual = args.Length;
            if (expected.Contains(actual)) return true;

            var expectedString = expected.Length == 1 ? expected[0].ToString() : string.Join(" or ", expected);

            i.SetResult($"Wrong number of arguments to command '{args[0]}': expected {expectedString}, got {args.Length}");
            return false;
        }

        private static long BoolToLong(bool value) => value ? 1L : 0L;
    }
}
