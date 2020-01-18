using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Delta.Slang.Semantic
{
    [ExcludeFromCodeCoverage]
    public class OverloadingTests
    {
        [Fact]
        public void Declaring_Overloads_compiles()
        {
            const string program = @"
fun f(i: int): void { }
fun f(s: string): void { }
";
            var result = Helpers.Bind(program);
            Assert.Equal(2, result.tree.Scope.LookupFunctions("f").Count());
            Assert.Empty(result.diagnostics);
        }

        [Fact]
        public void Invoking_Overload_with_correct_types_compiles()
        {
            const string program = @"
fun f(i: int): void { }
fun f(s: string): void { }
fun foo(): void {
    f(42);
    f(""Hello"");
}
";
            var result = Helpers.Bind(program);
            Assert.Equal(2, result.tree.Scope.LookupFunctions("f").Count());
            Assert.Empty(result.diagnostics);
        }
        
        [Fact]
        public void Invoking_Overload_with_implicitly_converted_types_compiles()
        {
            const string program = @"
fun f(i: int): void { }
fun f(s: string): void { }
fun foo(): void {
    f(42.0);
}
";
            var result = Helpers.Bind(program);
            Assert.Equal(2, result.tree.Scope.LookupFunctions("f").Count());
            Assert.Empty(result.diagnostics);
        }
    }
}
