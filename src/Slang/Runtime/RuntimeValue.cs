using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Slang.Runtime
{
    public sealed class RuntimeValue
    {
        private object? content;

        public RuntimeValue(object? value = null) => Set(value);

        public static RuntimeValue Null { get; } = new RuntimeValue();

        public bool IsNull() => content == null;                
        public bool IsNumber() => content is int or double;

        public bool IsBool() => content is bool;
        public bool IsBool([NotNullWhen(true)] out bool? value)
        {
            if (content is bool b)
            {
                value = b;
                return true;
            }

            value = null;
            return false;
        }

        public bool IsInt([NotNullWhen(true)] out int? value)
        {
            if (content is int i)
            {
                value = i;
                return true;
            }

            value = null;
            return false;
        }

        // NB: if the value is an int, it is casted to a double and this method returns true
        public bool IsDouble([NotNullWhen(true)] out double? value)
        {
            if (content is int i)
            {
                value = i;
                return true;
            }

            if (content is double d)
            {
                value = d;
                return true;
            }

            value = null;
            return false;
        }

        public bool IsString() => content is string;
        public bool IsString([NotNullWhen(true)] out string? text)
        {
            if (content is string s)
            {
                text = s;
                return true;
            }

            text = null;
            return false;
        }

        public object? Get() => content;
        public void Set(object? value) => content = value;

        public override string ToString()
        {
            if (content == null) return "<null>";
            if (content is string s) return s;
            if (content is double d) return d.ToString(CultureInfo.InvariantCulture);
            return content.ToString() ?? "<null>";
        }
    }
}
