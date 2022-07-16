using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Delta.Slang.Syntax
{
    [ExcludeFromCodeCoverage]
    public class LexerTests
    {
        [Fact]
        public void lex_simple_function_call()
        {
            const string program =  "print(\"Hello, World!\");";
            var result = Helpers.Lex(program);
            Assert.Empty(result.diagnostics);
        }
    }
}
