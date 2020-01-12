using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Delta.Slang.Semantic
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
