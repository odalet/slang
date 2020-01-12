using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Delta.Slang.Syntax
{
    [ExcludeFromCodeCoverage]
    public class ParserTests
    {
        [Fact]
        public void parse_simple_function_call()
        {
            const string program = "print(\"Hello, World!\");";
            var result = Helpers.Parse(program);
            Assert.Empty(result.diagnostics);
        }
    }
}
