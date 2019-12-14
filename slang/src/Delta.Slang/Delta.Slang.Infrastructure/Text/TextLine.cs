using System;

namespace Delta.Slang.Text
{
    // Copied from Roslyn
    public readonly struct TextLine : IEquatable<TextLine>
    {
        private TextLine(SourceText text, int start, int endIncludingBreaks)
        {
            Text = text;
            Start = start;
            EndIncludingLineBreak = endIncludingBreaks;
        }

        /// <summary>
        /// Creates a <see cref="TextLine"/> instance.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="span">The span of the line.</param>
        /// <returns>An instance of <see cref="TextLine"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The span does not represent a text line.</exception>
        public static TextLine FromSpan(SourceText text, TextSpan span)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (span.Start > text.Length || span.Start < 0 || span.End > text.Length)
                throw new ArgumentOutOfRangeException(nameof(span));

            if (text.Length <= 0)
                return new TextLine(text, 0, 0);

            // check span is start of line
            if (span.Start > 0 && !TextUtils.IsLineBreak(text[span.Start - 1]))
                throw new ArgumentOutOfRangeException(nameof(span), "Span does not include Start of Line");

            var endIncludesLineBreak = false;
            if (span.End > span.Start)
                endIncludesLineBreak = TextUtils.IsLineBreak(text[span.End - 1]);

            if (!endIncludesLineBreak && span.End < text.Length)
            {
                var lineBreakLength = TextUtils.GetLengthOfLineBreak(text, span.End);
                if (lineBreakLength > 0)
                {
                    // adjust span to include line breaks
                    endIncludesLineBreak = true;
                    span = new TextSpan(span.Start, span.Length + lineBreakLength);
                }
            }

            // check end of span is at end of line
            if (span.End < text.Length && !endIncludesLineBreak)
                throw new ArgumentOutOfRangeException(nameof(span), "Span does not include End of Line");

            return new TextLine(text, span.Start, span.End);
        }

        /// <summary>
        /// Gets the source text.
        /// </summary>
        public SourceText Text { get; }

        /// <summary>
        /// Gets the start position of the line.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the end position of the line including the line break.
        /// </summary>
        public int EndIncludingLineBreak { get; }

        /// <summary>
        /// Gets the zero-based line number.
        /// </summary>
        public int LineNumber => Text?.Lines.IndexOf(Start) ?? 0;

        /// <summary>
        /// Gets the end position of the line not including the line break.
        /// </summary>
        public int End => EndIncludingLineBreak - LineBreakLength;

        /// <summary>
        /// Gets the line span not including the line break.
        /// </summary>
        public TextSpan Span => TextSpan.FromBounds(Start, End);

        /// <summary>
        /// Gets the line span including the line break.
        /// </summary>
        public TextSpan SpanIncludingLineBreak => TextSpan.FromBounds(Start, EndIncludingLineBreak);

        private int LineBreakLength
        {
            get
            {
                if (Text == null || Text.Length == 0 || EndIncludingLineBreak == Start)
                    return 0;

                return TextUtils.GetStartAndLengthOfLineBreakEndingAt(Text, EndIncludingLineBreak - 1).length;
            }
        }

        public override string ToString()
        {
            if (Text == null || Text.Length == 0)
                return string.Empty;
            return Text.ToString(Span);
        }

        public bool Equals(TextLine other) => other.Text == Text && other.Start == Start && other.EndIncludingLineBreak == EndIncludingLineBreak;
        public override bool Equals(object obj) => obj is TextLine line && Equals(line);
        public override int GetHashCode() => HashUtils.Combine(Text, HashUtils.Combine(Start, EndIncludingLineBreak));

        public static bool operator ==(TextLine left, TextLine right) => left.Equals(right);
        public static bool operator !=(TextLine left, TextLine right) => !left.Equals(right);
    }
}
