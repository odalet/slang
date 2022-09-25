using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Slang.Interpreter
{
    using static Helpers;

    [ExcludeFromCodeCoverage]
    public class IfTests
    {
        [Theory]
        // else is always associated with the nearest if
        [InlineData("if (true) if (false) print(\"A\"); else print(\"B\");", "B")]
        [InlineData("if (false) if (false) print(\"A\"); else print(\"B\");", "")]
        // Braces have no impact
        [InlineData("if (true) print(\"A\");", "A")]
        [InlineData("if (true) { print(\"A\"); }", "A")]
        [InlineData("if (true) {{{ print(\"A\"); }}}", "A")]
        // no else or else with empty block or empty statement are equivalent        
        [InlineData("if (false) print(\"A\");", "")]
        [InlineData("if (false) print(\"A\"); else {}", "")]
        [InlineData("if (false) print(\"A\"); else {{{}}}", "")]
        [InlineData("if (false) print(\"A\"); else ;", "")]
        [InlineData("if (false) print(\"A\"); else ;;;", "")]
        [InlineData("if (false) print(\"A\"); else {{{;;;}}}", "")]
        public void If_tests(string source, string expected)
        {
            var actual = Interpret(source, out var diagnostics);
            Assert.Equal(expected, actual);
            Assert.Empty(diagnostics);
        }
    }
}
