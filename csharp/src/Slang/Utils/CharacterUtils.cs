using System.Globalization;

namespace Slang.Utils
{
    // Pretty much copied from Roslyn's CharacterInfo.cs
    internal static class CharacterUtils
    {
        // whitespace:
        //   Any character with Unicode class Zs
        //   Horizontal tab character (U+0009)
        //   Vertical tab character (U+000B)
        //   Form feed character (U+000C)
        //   NO-BREAK SPACE (U+00A0)
        // NB:
        //    Space and no-break space are the only space separators (Zs) in ASCII range
        // NO-BREAK SPACE ('\u00A0')
        // The native compiler, in ScanToken, recognized both the byte-order
        // marker '\uFEFF' as well as ^Z '\u001A' as whitespace, although
        // this is not to spec since neither of these are in Zs. For the
        // sake of compatibility, we recognize them both here. Note: '\uFEFF'
        // also happens to be a formatting character (class Cf), which means
        // that it is a legal non-initial identifier character. So it's
        // especially funny, because it will be whitespace UNLESS we happen
        // to be scanning an identifier or keyword, in which case it winds
        // up in the identifier or keyword.
        public static bool IsWhitespace(in char ch) =>
            ch is ' ' or '\t' or '\v' or '\f' or '\u00A0' ||
            ch > 255 && CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator;

        // new-line-character:
        //   Carriage return character (U+000D)
        //   Line feed character (U+000A)
        //   Next line character (U+0085)
        //   Line separator character (U+2028)
        //   Paragraph separator character (U+2029)
        public static bool IsNewLine(in char ch) =>
            ch is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029';
        
        public static bool IsDigit(char ch) => ch is >= '0' and <= '9';
    }
}
