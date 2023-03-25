using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Delta.Slang.Semantics
{
    [ExcludeFromCodeCoverage]
    public class BinderTests
    {
        [Fact]
        public void bind_simple_function_call()
        {
            const string program = "print(\"Hello, World!\");";
            var result = Helpers.Bind(program);
            Assert.Empty(result.diagnostics);
        }

        [Fact]
        public void function_can_be_called_with_parameters_with_exact_type()
        {
            const string program = @"
fun f(i: int): void { }
fun foo(): void {
    f(42);
}
";
            var result = Helpers.Bind(program);
            Assert.Empty(result.diagnostics);
        }

        [Fact]
        public void function_can_be_called_with_parameters_with_compatible_type()
        {
            const string program = @"
fun f(d: double): void { }
fun foo(): void {
    f(42);
}
";
            var result = Helpers.Bind(program);
            Assert.Empty(result.diagnostics);
        }

        [Fact]
        public void type_of_double_plus_double_is_double()
        {
            const string program = "var d = 1.5 + 1.5;";
            var result = Helpers.Bind(program);
            Assert.Empty(result.diagnostics);
        }

        [Fact]
        public void type_of_double_plus_int_is_double()
        {
            const string program = "var d = 1.5 + 1;";
            var result = Helpers.Bind(program);
            Assert.Empty(result.diagnostics);
        }

        [Fact]
        public void type_of_int_plus_double_is_double()
        {
            const string program = "var d = 1 + 1.5;";
            var result = Helpers.Bind(program);
            Assert.Empty(result.diagnostics);
        }
    }
}
