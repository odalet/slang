using System.Runtime.CompilerServices;

namespace Slang.CodeAnalysis.Text
{
    // Copied from Roslyn
    internal static class TextUtils
    {
        // Note: a small amount of the below logic is also inlined into SourceText.ParseLineBreaks for performance reasons.
        public static int GetLengthOfLineBreak(SourceText text, int index)
        {
            var c = text[index];

            // common case - ASCII & not a line break:
            // c > '\r' && c <= 127 -> c >= ('\r'+1) && c <= 127
            const uint bias = '\r' + 1;
            return unchecked(c - bias) <= 127 - bias ? 0 : getLengthOfLineBreakSlow(text, index, c);

            static int getLengthOfLineBreakSlow(SourceText text, int index, char c)
            {
                if (c == '\r')
                {
                    var next = index + 1;
                    return next < text.Length && '\n' == text[next] ? 2 : 1;
                }

                return IsAnyLineBreakCharacter(c) ? 1 : 0;
            }
        }

        /// <summary>
        /// Returns startLineBreak = index-1, lengthLineBreak = 2   if there is a \r\n at index-1
        /// Returns startLineBreak = index,   lengthLineBreak = 1   if there is a 1-char newline at index
        /// Returns startLineBreak = index+1, lengthLineBreak = 0   if there is no newline at index.
        /// </summary>
        public static (int start, int length) GetStartAndLengthOfLineBreakEndingAt(SourceText text, int index)
        {
            var c = text[index];

            if (c == '\n' && index > 0 && text[index - 1] == '\r')
            {
                // "\r\n" is the only 2-character line break.
                return index > 0 && text[index - 1] == '\r' ? (index - 1, 2) : (index, 1);
            }

            return IsAnyLineBreakCharacter(c) ? (index, 1) : (index + 1, 0);
        }

        // NB: these are:
        // CR,
        // LF,
        // NEXT LINE (http://www.fileformat.info/info/unicode/char/85/index.htm)
        // LINE SEPARATOR (http://www.fileformat.info/info/unicode/char/2028/index.htm) and
        // PARAGRAPH SEPARATOR (http://www.fileformat.info/info/unicode/char/2029/index.htm)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyLineBreakCharacter(char c) =>
            c is '\n' or '\r' or '\u0085' or '\u2028' or '\u2029';
    }
}
