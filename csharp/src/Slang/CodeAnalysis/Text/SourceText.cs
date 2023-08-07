using System;
using System.Text;
using System.Threading;

namespace Slang.CodeAnalysis.Text;

// Simplified from Roslyn's SourceText
public abstract partial class SourceText
{
    private static readonly Encoding utf8EncodingWithNoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
    private TextLineCollection? lazyLineInfo;

    protected SourceText() { }

    /// <summary>
    /// Encoding of the file that the text was read from or is going to be saved to.
    /// <c>null</c> if the encoding is unspecified.
    /// </summary>
    /// <remarks>
    /// If the encoding is not specified the source isn't debuggable.
    /// If an encoding-less <see cref="SourceText"/> is written to a file a <see cref="Encoding.UTF8"/> shall be used as a default.
    /// </remarks>
    public abstract Encoding? Encoding { get; }

    /// <summary>
    /// The length of the text in characters.
    /// </summary>
    public abstract int Length { get; }

    /// <summary>
    /// Returns a character at given position.
    /// </summary>
    /// <param name="position">The position to get the character from.</param>
    /// <returns>The character.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When position is negative or greater than <see cref="Length"/>.</exception>
    public abstract char this[int position] { get; }

    /// <summary>
    /// The collection of individual text lines.
    /// </summary>
    public TextLineCollection Lines
    {
        get
        {
            var info = lazyLineInfo;
            return info ?? Interlocked.CompareExchange(ref lazyLineInfo, info = CreateLineInfo(), null) ?? info;
        }
    }

    /// <summary>
    /// Copy a range of characters from this SourceText to a destination array.
    /// </summary>
    public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

    /// <summary>
    /// Gets a string containing the characters in specified span.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">When given span is outside of the text range.</exception>
    public abstract string ToString(TextSpan span);

    /// <summary>
    /// Provides a string representation of the SourceText.
    /// </summary>
    public override string ToString() => ToString(new TextSpan(0, Length));

    private static bool IsBinary(string text) => IsBinary(text.AsSpan());

    // NB: On .NET Core, Contains has an optimized vectorized implementation, much faster than a custom loop.
    private static bool IsBinary(ReadOnlySpan<char> text) => text.Contains("\0\0", StringComparison.Ordinal);
}
