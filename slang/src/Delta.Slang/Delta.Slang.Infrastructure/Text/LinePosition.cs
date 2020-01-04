using System;

namespace Delta.Slang.Text
{
    // Copied from Roslyn
    /// <summary>
    /// Immutable representation of a line number and position within a SourceText instance.
    /// </summary>
    public readonly struct LinePosition : IEquatable<LinePosition>, IComparable<LinePosition>
    {
        /// <summary>
        /// A <see cref="LinePosition"/> that represents position 0 at line 0.
        /// </summary>
        public static LinePosition Zero => default;

        /// <summary>
        /// Initializes a new instance of a <see cref="LinePosition"/> with the given line and character.
        /// </summary>
        /// <param name="line">
        /// The line of the line position. The first line in a file is defined as line 0 (zero based line numbering).
        /// </param>
        /// <param name="column">
        /// The character position in the line.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="line"/> or <paramref name="column"/> is less than zero. </exception>
        public LinePosition(int line, int column)
        {
            if (line < 0) throw new ArgumentOutOfRangeException(nameof(line));
            if (column < 0) throw new ArgumentOutOfRangeException(nameof(column));

            Line = line;
            Column = column;
        }
        
        /// <summary>
        /// The line number. The first line in a file is defined as line 0 (zero based line numbering).
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// The character position within the line (the column).
        /// </summary>
        public int Column { get; }

        public override string ToString() => $"{Line},{Column}";

        public bool Equals(LinePosition other) => other.Line == Line && other.Column == Column;
        public override bool Equals(object obj) => obj is LinePosition position && Equals(position);
        public override int GetHashCode() => HashUtils.Combine(Line, Column);
        
        public int CompareTo(LinePosition other)
        {
            var result = Line.CompareTo(other.Line);
            return result != 0 ? result : Column.CompareTo(other.Column);
        }

        public static bool operator ==(LinePosition left, LinePosition right) => left.Equals(right);
        public static bool operator !=(LinePosition left, LinePosition right) => !left.Equals(right);
        public static bool operator >(LinePosition left, LinePosition right) => left.CompareTo(right) > 0;
        public static bool operator >=(LinePosition left, LinePosition right) => left.CompareTo(right) >= 0;
        public static bool operator <(LinePosition left, LinePosition right) => left.CompareTo(right) < 0;
        public static bool operator <=(LinePosition left, LinePosition right) => left.CompareTo(right) <= 0;
    }
}
