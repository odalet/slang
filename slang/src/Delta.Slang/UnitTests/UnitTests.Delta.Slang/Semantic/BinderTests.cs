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
    }
}
