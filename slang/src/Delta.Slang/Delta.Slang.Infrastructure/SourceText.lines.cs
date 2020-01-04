using System;
using Delta.Slang.Pooling;
using Delta.Slang.Text;

namespace Delta.Slang
{
    partial class SourceText
    {
        private sealed class LineInfo : TextLineCollection
        {
            private readonly SourceText text;
            private readonly int[] starts;
            private int lastLineNumber;

            public LineInfo(SourceText sourceText, int[] lineStartsArray)
            {
                text = sourceText;
                starts = lineStartsArray;
            }

            public override int Count => starts.Length;

            public override TextLine this[int index]
            {
                get
                {
                    if (index < 0 || index >= starts.Length)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    var start = starts[index];
                    if (index == starts.Length - 1)
                        return TextLine.FromSpan(text, TextSpan.FromBounds(start, text.Length));

                    var end = starts[index + 1];
                    return TextLine.FromSpan(text, TextSpan.FromBounds(start, end));
                }
            }

            public override int IndexOf(int position)
            {
                if (position < 0 || position > text.Length)
                    throw new ArgumentOutOfRangeException(nameof(position));

                int currentLineNumber;

                // it is common to ask about position on the same line as before or on the next couple lines
                var last = lastLineNumber;
                if (position >= starts[last])
                {
                    var limit = Math.Min(starts.Length, last + 4);
                    for (var i = last; i < limit; i++)
                    {
                        if (position < starts[i])
                        {
                            currentLineNumber = i - 1;
                            lastLineNumber = currentLineNumber;
                            return currentLineNumber;
                        }
                    }
                }

                // Binary search to find the right line. If no lines start exactly at position, 
                // round to the left. EOF position will map to the last line.
                currentLineNumber = starts.BinarySearch(position);
                if (currentLineNumber < 0)
                    currentLineNumber = ~currentLineNumber - 1;

                lastLineNumber = currentLineNumber;
                return currentLineNumber;
            }

            public override TextLine GetLineFromPosition(int position) => this[IndexOf(position)];
        }

        private const int CharBufferSize = 32 * 1024;
        private const int CharBufferCount = 5;

        private static readonly ObjectPool<char[]> charArrayPool = new ObjectPool<char[]>(() => new char[CharBufferSize], CharBufferCount);

        private LineInfo CreateLineInfo() => new LineInfo(this, ParseLineStarts());

        private void EnumerateChars(Action<int, char[], int> action)
        {
            var position = 0;
            var buffer = charArrayPool.Allocate();

            var length = Length;
            while (position < length)
            {
                var contentLength = Math.Min(length - position, buffer.Length);
                CopyTo(position, buffer, 0, contentLength);
                action(position, buffer, contentLength);
                position += contentLength;
            }

            // once more with zero length to signal the end
            action(position, buffer, 0);

            charArrayPool.Free(buffer);
        }

        private int[] ParseLineStarts()
        {
            // Corner case check
            if (0 == Length) return new[] { 0 };

            var lineStarts = ArrayBuilder<int>.GetInstance();
            lineStarts.Add(0); // there is always the first line

            var lastWasCR = false;

            // The following loop goes through every character in the text. It is highly
            // performance critical, and thus inlines knowledge about common line breaks
            // and non-line breaks.
            EnumerateChars((int position, char[] buffer, int length) =>
            {
                var index = 0;
                if (lastWasCR)
                {
                    if (length > 0 && buffer[0] == '\n')
                        index++;

                    lineStarts.Add(position + index);
                    lastWasCR = false;
                }

                while (index < length)
                {
                    var current = buffer[index];
                    index++;

                    // Common case - ASCII & not a line break
                    // if (c > '\r' && c <= 127)
                    // if (c >= ('\r'+1) && c <= 127)
                    const uint bias = '\r' + 1;
                    if (unchecked(current - bias) <= 127 - bias)
                        continue;

                    // Assumes that the only 2-char line break sequence is CR+LF
                    if (current == '\r')
                    {
                        if (index < length && buffer[index] == '\n')
                            index++;
                        else if (index >= length)
                        {
                            lastWasCR = true;
                            continue;
                        }
                    }
                    else if (!TextUtils.IsLineBreak(current))
                        continue;

                    // next line starts at index
                    lineStarts.Add(position + index);
                }
            });

            return lineStarts.ToArrayAndFree();
        }
    }
}
