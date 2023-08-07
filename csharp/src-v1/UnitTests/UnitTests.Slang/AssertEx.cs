using System.Collections.Generic;
using Slang.CodeAnalysis.Syntax;
using Xunit.Sdk;

namespace Slang;

public static class AssertEx
{
    private enum MatchResult
    {
        Success,
        MoreInActual,
        MoreInExpected
    }

    public static void KindIs(IEnumerable<SyntaxKind> expected, IEnumerable<Token> actual)
    {
        var result = Match(expected, actual);
        if (result == MatchResult.Success)
            return;

        var more = result == MatchResult.MoreInActual ? nameof(actual) : nameof(expected);
        throw new XunitException($"There are items in '{more}' that could not be matched");
    }

    private static MatchResult Match(IEnumerable<SyntaxKind> expected, IEnumerable<Token> actual)
    {
        var expEnumerator = expected.GetEnumerator();
        var actEnumerator = actual.GetEnumerator();
        var i = 0;
        while (true)
        {
            var endOfExpected = !expEnumerator.MoveNext();
            var endOfActual = !actEnumerator.MoveNext();

            if (endOfActual && endOfExpected) return MatchResult.Success;
            if (endOfActual) return MatchResult.MoreInExpected;
            if (endOfExpected) return MatchResult.MoreInActual;

            var exp = expEnumerator.Current;
            var act = actEnumerator.Current;
            if (act.Kind != exp)
                throw new XunitException($"Token Kinds are different at index {i}; Expected: {exp}, Actual: {act}");

            i++;
        }
    }
}
