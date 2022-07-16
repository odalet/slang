using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Delta.Slang.Text
{
    // Copied from Roslyn
    public abstract class TextLineCollection : IReadOnlyList<TextLine>
    {
        [SuppressMessage("Performance", "CA1067", Justification = "Equality not actually implemented")]
        private struct Enumerator : IEnumerator<TextLine>, IEnumerator
        {
            private readonly TextLineCollection lines;
            private int index;

            internal Enumerator(TextLineCollection collection, int initialIndex = -1)
            {
                lines = collection;
                index = initialIndex;
            }

            public TextLine Current
            {
                get
                {
                    var localIndex = index;
                    return localIndex >= 0 && localIndex < lines.Count ? lines[localIndex] : (default);
                }
            }

            public bool MoveNext()
            {
                if (index < lines.Count - 1)
                {
                    index++;
                    return true;
                }

                return false;
            }

            object IEnumerator.Current => Current;
            bool IEnumerator.MoveNext() => MoveNext();
            void IEnumerator.Reset() { /* Intentionally not implemented */ }
            void IDisposable.Dispose() { /* Nothing to dispose */ }

            public override bool Equals(object obj) => throw new NotSupportedException("Not supported");
            public override int GetHashCode() => throw new NotSupportedException("Not supported");
        }

        /// <summary>
        /// The count of <see cref="TextLine"/> items in the collection
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets the <see cref="TextLine"/> item at the specified index.
        /// </summary>
        public abstract TextLine this[int index] { get; }

        /// <summary>
        /// The index of the TextLine that encompasses the character position.
        /// </summary>
        public abstract int IndexOf(int position);

        /// <summary>
        /// Gets a <see cref="TextLine"/> that encompasses the character position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public virtual TextLine GetLineFromPosition(int position) => this[IndexOf(position)];

        /// <summary>
        /// Gets a <see cref="LinePosition"/> corresponding to a character position.
        /// </summary>
        public virtual LinePosition GetLinePosition(int position)
        {
            var line = GetLineFromPosition(position);
            return new LinePosition(line.LineNumber, position - line.Start);
        }

        /// <summary>
        /// Convert a <see cref="TextSpan"/> to a <see cref="LinePositionSpan"/>.
        /// </summary>
        public LinePositionSpan GetLinePositionSpan(TextSpan span) =>
            new LinePositionSpan(GetLinePosition(span.Start), GetLinePosition(span.End));

        /// <summary>
        /// Convert a <see cref="LinePosition"/> to a position.
        /// </summary>
        public int GetPosition(LinePosition position) => this[position.Line].Start + position.Column;

        /// <summary>
        /// Convert a <see cref="LinePositionSpan"/> to <see cref="TextSpan"/>.
        /// </summary>
        public TextSpan GetTextSpan(LinePositionSpan span) =>
            TextSpan.FromBounds(GetPosition(span.Start), GetPosition(span.End));

        private Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<TextLine> IEnumerable<TextLine>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
