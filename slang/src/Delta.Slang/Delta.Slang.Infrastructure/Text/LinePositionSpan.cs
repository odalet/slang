using System;

namespace Delta.Slang.Text
{
    // Copied from Roslyn
    /// <summary>
    /// Immutable span represented by a pair of line number and index within the line.
    /// </summary>
    public readonly struct LinePositionSpan : IEquatable<LinePositionSpan>
    {
        /// <summary>
        /// Creates <see cref="LinePositionSpan"/>.
        /// </summary>
        /// <param name="start">Start position.</param>
        /// <param name="end">End position.</param>
        /// <exception cref="ArgumentException"><paramref name="end"/> precedes <paramref name="start"/>.</exception>
        public LinePositionSpan(LinePosition start, LinePosition end)
        {
            if (end < start)
                throw new ArgumentException("End must not be less than Start", nameof(end));

            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets the start position of the span.
        /// </summary>
        public LinePosition Start { get; }

        /// <summary>
        /// Gets the end position of the span.
        /// </summary>
        public LinePosition End { get; }

        /// <summary>
        /// Provides a string representation for <see cref="LinePositionSpan"/>.
        /// </summary>
        /// <example>(0,0)-(5,6)</example>
        public override string ToString() => $"({Start})-({End})";

        public bool Equals(LinePositionSpan other) => Start.Equals(other.Start) && End.Equals(other.End);
        public override bool Equals(object obj) => obj is LinePositionSpan span && Equals(span);
        public override int GetHashCode() => HashUtils.Combine(Start.GetHashCode(), End.GetHashCode());

        public static bool operator ==(LinePositionSpan left, LinePositionSpan right) => left.Equals(right);
        public static bool operator !=(LinePositionSpan left, LinePositionSpan right) => !left.Equals(right);
    }
}