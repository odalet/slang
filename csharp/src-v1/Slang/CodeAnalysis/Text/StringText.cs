using System;
using System.Text;

namespace Slang.CodeAnalysis.Text;

// Simplified from Roslyn's StringText
internal sealed class StringText : SourceText
{
    internal StringText(string source, Encoding? encoding) : base()
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Encoding = encoding;
    }

    /// <summary>
    /// Underlying string which is the source of this <see cref="StringText"/>instance
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Encoding of the file that the text was read from or is going to be saved to.
    /// <c>null</c> if the encoding is unspecified.
    /// </summary>
    /// <remarks>
    /// If the encoding is not specified the source isn't debuggable.
    /// If an encoding-less <see cref="SourceText" /> is written to a file a <see cref="Encoding.UTF8" /> shall be used as a default.
    /// </remarks>
    public override Encoding? Encoding { get; }

    /// <summary>
    /// The length of the text represented by <see cref="StringText"/>.
    /// </summary>
    public override int Length => Source.Length;

    /// <summary>
    /// Returns a character at given position.
    /// </summary>
    /// <remarks>
    /// We are not validating position here as that would not add any value to the range check that string accessor performs anyways.
    /// </remarks>
    /// <param name="position">The position to get the character from.</param>
    /// <returns>The character.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When position is negative or greater than <see cref="Length"/>.</exception>
    public override char this[int position] => Source[position];

    /// <summary>
    /// Provides a string representation of the StringText located within given span.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">When given span is outside of the text range.</exception>
    public override string ToString(TextSpan span)
    {
        if (span.End > Source.Length) throw new ArgumentOutOfRangeException(nameof(span));

        return span.Start == 0 && span.Length == Length 
            ? Source 
            : Source.Substring(span.Start, span.Length);
    }

    public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count) =>
        Source.CopyTo(sourceIndex, destination, destinationIndex, count);
}
