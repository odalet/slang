using System;
using System.Runtime.CompilerServices;

namespace Slang.CodeAnalysis.Text
{
    // Heavily inspired by (well... copied from) Roslyn's TextSpan
    public readonly struct TextSpan : IEquatable<TextSpan>, IComparable<TextSpan>
    {
        public static TextSpan Zero => default;

        /// <summary>
        /// Creates a TextSpan instance beginning with the position Start and having the Length
        /// specified with <paramref name="length" />.
        /// </summary>
        public TextSpan(int start, int length)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (start + length < start) throw new ArgumentOutOfRangeException(nameof(length));

            Start = start;
            Length = length;
        }

        /// <summary>
        /// Start point of the span.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// End of the span.
        /// </summary>
        public int End => Start + Length;

        /// <summary>
        /// Length of the span.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Determines whether or not the span is empty.
        /// </summary>
        public bool IsEmpty => Length == 0;

        /// <summary>
        /// Creates a new <see cref="TextSpan"/> from <paramref name="start" /> and <paramref
        /// name="end"/> positions as opposed to a position and length.
        /// </summary>
        /// <remarks>
        /// The returned TextSpan contains the range with <paramref name="start"/> inclusive, 
        /// and <paramref name="end"/> exclusive.
        /// </remarks>
        public static TextSpan FromBounds(int start, int end)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (end < start) throw new ArgumentOutOfRangeException(nameof(end));
            return new TextSpan(start, end - start);
        }

        /// <summary>
        /// Determines whether the position lies within the span.
        /// </summary>
        /// <param name="position">
        /// The position to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the position is greater than or equal to Start and strictly less 
        /// than End, otherwise <c>false</c>.
        /// </returns>
        public bool Contains(int position) => unchecked((uint)(position - Start) < (uint)Length);

        /// <summary>
        /// Determines whether <paramref name="span"/> falls completely within this span.
        /// </summary>
        /// <param name="span">
        /// The span to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified span falls completely within this span, otherwise <c>false</c>.
        /// </returns>
        public bool Contains(TextSpan span) => span.Start >= Start && span.End <= End;

        /// <summary>
        /// Determines whether <paramref name="span"/> overlaps this span. Two spans are considered to overlap 
        /// if they have positions in common and neither is empty. Empty spans do not overlap with any 
        /// other span.
        /// </summary>
        /// <param name="span">
        /// The span to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the spans overlap, otherwise <c>false</c>.
        /// </returns>
        public bool OverlapsWith(TextSpan span)
        {
            var (start, end) = GetOverlap(span);
            return start < end;
        }

        /// <summary>
        /// Returns the overlap with the given span, or null if there is no overlap.
        /// </summary>
        /// <param name="span">
        /// The span to check.
        /// </param>
        /// <returns>
        /// The overlap of the spans, or null if the overlap is empty.
        /// </returns>
        public TextSpan? Overlap(TextSpan span)
        {
            var (start, end) = GetOverlap(span);
            return start < end ? TextSpan.FromBounds(start, end) : (TextSpan?)null;
        }

        /// <summary>
        /// Compares current instance of <see cref="TextSpan"/> with another.
        /// </summary>
        public int CompareTo(TextSpan other)
        {
            var diff = Start - other.Start;
            return diff != 0 ? diff : Length - other.Length;
        }

        public override bool Equals(object? obj) => obj is TextSpan span && Equals(span);

        public bool Equals(TextSpan other) => Start == other.Start && Length == other.Length;

        public override int GetHashCode() => HashCode.Combine(Start, Length);

        /// <summary>
        /// Provides a string representation for <see cref="TextSpan"/>.
        /// </summary>
        public override string ToString() => $"[{Start}..{End})";

        public static bool operator ==(TextSpan left, TextSpan right) => left.Equals(right);
        public static bool operator !=(TextSpan left, TextSpan right) => !left.Equals(right);
        public static bool operator <(TextSpan left, TextSpan right) => left.CompareTo(right) < 0;
        public static bool operator >(TextSpan left, TextSpan right) => left.CompareTo(right) > 0;
        public static bool operator <=(TextSpan left, TextSpan right) => left.CompareTo(right) <= 0;
        public static bool operator >=(TextSpan left, TextSpan right) => left.CompareTo(right) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int start, int end) GetOverlap(TextSpan span) => (Math.Max(Start, span.Start), Math.Min(End, span.End));
    }
}
