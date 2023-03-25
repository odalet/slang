using System;
using System.Text;

namespace Delta.Slang.Text
{
    // Copied from Roslyn
    internal static class TextUtils
    {
        // Note: a small amount of this below logic is also inlined into SourceText.ParseLineBreaks
        // for performance reasons.
        public static int GetLengthOfLineBreak(SourceText text, int index)
        {
            var c = text[index];

            // common case - ASCII & not a line break
            // if (c > '\r' && c <= 127)
            // if (c >= ('\r'+1) && c <= 127)
            const uint bias = '\r' + 1;
            if (unchecked(c - bias) <= (127 - bias))
                return 0;

            return GetLengthOfLineBreakSlow(text, index, c);
        }

        private static int GetLengthOfLineBreakSlow(SourceText text, int index, char c)
        {
            if (c == '\r')
            {
                var next = index + 1;
                return (next < text.Length) && '\n' == text[next] ? 2 : 1;
            }

            if (IsLineBreak(c)) return 1;
            return 0;
        }

        /// <summary>
        /// Returns startLineBreak = index-1, lengthLineBreak = 2   if there is a \r\n at index-1
        /// Returns startLineBreak = index,   lengthLineBreak = 1   if there is a 1-char newline at index
        /// Returns startLineBreak = index+1, lengthLineBreak = 0   if there is no newline at index.
        /// </summary>
        public static (int start, int length) GetStartAndLengthOfLineBreakEndingAt(SourceText text, int index)
        {
            char c = text[index];

            if (c == '\n' && index > 0 && text[index - 1] == '\r')
            {
                // "\r\n" is the only 2-character line break.
                if (index > 0 && text[index - 1] == '\r')
                    return (index - 1, 2);

                return (index, 1);
            }

            if (IsLineBreak(c))
                return (index, 1);

            return (index + 1, 0);
        }

        /// <summary>
        /// Determines whether the specified character is a line break character
        /// </summary>
        public static bool IsLineBreak(char c) =>
            c == '\r' || // CR
            c == '\n' || // LF
            c == '\u0085' || // NEL - NEXT LINE (http://www.fileformat.info/info/unicode/char/85/index.htm)
            c == '\u2028' || // LINE SEPARATOR (http://www.fileformat.info/info/unicode/char/2028/index.htm) 
            c == '\u2029'; // PARAGRAPH SEPARATOR (http://www.fileformat.info/info/unicode/char/2029/index.htm)
        
        /// <summary>
        /// Check for occurrence of two consecutive NUL (U+0000) characters.
        /// This is unlikely to appear in genuine text, so it's a good heuristic
        /// to detect binary files.
        /// </summary>
        public static bool IsBinary(string text)
        {
            // PERF: We can advance two chars at a time unless we find a NUL.
            for (var i = 1; i < text.Length;)
            {
                if (text[i] == '\0')
                {
                    if (text[i - 1] == '\0')
                        return true;

                    i++;
                }
                else i += 2;
            }

            return false;
        }

        /// <summary>
        /// Detects an encoding by looking for byte order marks.
        /// </summary>
        /// <param name="source">A buffer containing the encoded text.</param>
        /// <param name="length">The length of valid data in the buffer.</param>
        /// <param name="preambleLength">The length of any detected byte order marks.</param>
        /// <returns>The detected encoding or null if no recognized byte order mark was present.</returns>
        public static Encoding TryReadByteOrderMark(byte[] source, int length, out int preambleLength)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (length > source.Length) throw new ArgumentException(nameof(length));

            if (length >= 2)
            {
                switch (source[0])
                {
                    case 0xFE:
                        if (source[1] == 0xFF)
                        {
                            preambleLength = 2;
                            return Encoding.BigEndianUnicode;
                        }
                        break;
                    case 0xFF:
                        if (source[1] == 0xFE)
                        {
                            preambleLength = 2;
                            return Encoding.Unicode;
                        }
                        break;
                    case 0xEF:
                        if (source[1] == 0xBB && length >= 3 && source[2] == 0xBF)
                        {
                            preambleLength = 3;
                            return Encoding.UTF8;
                        }
                        break;
                }
            }

            preambleLength = 0;
            return null;
        }
    }
}
