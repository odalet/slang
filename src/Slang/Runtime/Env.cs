using System.Collections.Generic;

namespace Slang.Runtime
{
    public readonly record struct ValueDescriptor(RuntimeValue Content, bool IsReadOnly);

    // Stores variables
    public sealed class Env
    {
        private readonly Dictionary<string, ValueDescriptor> values = new();

        public Env(Env? parent = null) => Parent = parent;

        public Env? Parent { get; }

        public void Declare(string name, RuntimeValue value, bool isReadOnly)
        {
            if (values.ContainsKey(name)) throw new RuntimeException($"Variable {name} is alreay declared");
            else values.Add(name, new(value, isReadOnly));
        }

        public RuntimeValue Get(string name)
        {
            if (values.ContainsKey(name))
                return values[name].Content;

            return Parent == null
                ? throw new RuntimeException($"Variable {name} is not declared")
                : Parent.Get(name);
        }

        public void Set(string name, RuntimeValue value)
        {
            if (!values.ContainsKey(name))
            {
                if (Parent == null) throw new RuntimeException($"Variable {name} is not declared");

                Parent.Set(name, value);
                return;
            }

            if (values[name].IsReadOnly) throw new RuntimeException($"Read-only variable {name} can only be initialized once");

            values[name] = new(value, false);
        }
    }
}
