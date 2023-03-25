using System;
using Slang.CodeAnalysis.Text;
using Slang.Utilities;

namespace Slang.CodeAnalysis.Syntax
{
    // Simplified from Roslyn
    public sealed class SlidingTextWindow : IDisposable
    {
        private const int defaultWindowLength = 2048;

        // In many cases, e.g. PeekChar, we need the ability to indicate that there are
        // no characters left and we have reached the end of the stream, or some other
        // invalid or not present character was asked for. Due to perf concerns, things
        // like nullable or out variables are not viable. Instead we need to choose a
        // char value which can never be legal.
        // 
        // In .NET, all characters are represented in 16 bits using the UTF-16 encoding.
        // Fortunately for us, there are a variety of different bit patterns which
        // are *not* legal UTF-16 characters. 0xffff (char.MaxValue) is one of these
        // characters -- a legal Unicode code point, but not a legal UTF-16 bit pattern.
        public const char InvalidCharacter = char.MaxValue; // NB: we must have a const here...

        // Example for the above variables:
        // The text starts at 0.
        // The window onto the text starts at basis.
        // The current character is at (basis + offset), AKA the current "Position".
        // The current lexeme started at (basis + lexemeStart), which is <= (basis + offset)
        // The current lexeme is the characters between the lexemeStart and the offset.

        private static readonly ObjectPool<char[]> windowPool = new(() => new char[defaultWindowLength]);

        public SlidingTextWindow(SourceText text)
        {
            Text = text;
            Offset = 0;
            CharacterWindow = windowPool.Allocate();
            LexemeRelativeStart = 0;

            Basis = 0;
            TextEnd = text.Length;
        }

        public SourceText Text { get; }

        /// <summary>
        /// The current absolute position in the text file.
        /// </summary>
        public int Position => Basis + Offset;

        /// <summary>
        /// The current offset inside the window (relative to the window start).
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// The buffer backing the current window.
        /// </summary>
        public char[] CharacterWindow { get; private set; }

        /// <summary>
        /// Returns the start of the current lexeme relative to the window start.
        /// </summary>
        public int LexemeRelativeStart { get; private set; }

        /// <summary>
        /// Number of characters in the character window.
        /// </summary>
        public int CharacterWindowCount { get; private set; }

        /// <summary>
        /// The absolute position of the start of the current lexeme in the given
        /// SourceText.
        /// </summary>
        public int LexemeStartPosition => Basis + LexemeRelativeStart;

        /// <summary>
        /// The number of characters in the current lexeme.
        /// </summary>
        public int Width => Offset - LexemeRelativeStart;

        private int Basis { get; set; } // Offset of the window relative to the SourceText start.
        private int TextEnd { get; } // Absolute end position

        /// <summary>
        /// Start parsing a new lexeme.
        /// </summary>
        public void Start() => LexemeRelativeStart = Offset;

        public void Reset(int position)
        {
            // if position is within already read character range then just use what we have
            var relative = position - Basis;
            if (relative >= 0 && relative <= CharacterWindowCount)
                Offset = relative;
            else
            {
                // we need to reread text buffer
                var amountToRead = Math.Min(Text.Length, position + CharacterWindow.Length) - position;
                amountToRead = Math.Max(amountToRead, 0);
                if (amountToRead > 0)
                    Text.CopyTo(position, CharacterWindow, 0, amountToRead);

                LexemeRelativeStart = 0;
                Offset = 0;
                Basis = position;
                CharacterWindowCount = amountToRead;
            }
        }

        // After reading <see cref=" InvalidCharacter"/>, a consumer can determine
        // if the InvalidCharacter was in the user's source or a sentinel.
        // Comments and string literals are allowed to contain any Unicode character.
        public bool IsReallyAtEnd() => Offset >= CharacterWindowCount && Position >= TextEnd;

        /// <summary>
        /// Advance the current position by one. No guarantee that this
        /// position is valid.
        /// </summary>
        public void AdvanceChar() => Offset++;

        /// <summary>
        /// Advance the current position by n. No guarantee that this position
        /// is valid.
        /// </summary>
        public void AdvanceChar(int n) => Offset += n;

        /// <summary>
        /// Grab the next character and advance the position.
        /// </summary>
        /// <returns>
        /// The next character, <see cref="InvalidCharacter" /> if there were no characters 
        /// remaining.
        /// </returns>
        public char NextChar()
        {
            var c = PeekChar();
            if (c != InvalidCharacter)
                AdvanceChar();
            return c;
        }

        /// <summary>
        /// Gets the next character if there are any characters in the 
        /// SourceText. May advance the window if we are at the end.
        /// </summary>
        /// <returns>
        /// The next character if any are available. InvalidCharacter otherwise.
        /// </returns>
        public char PeekChar()
        {
            if (Offset >= CharacterWindowCount && !MoreChars())
                return InvalidCharacter;

            // N.B. MoreChars may update the offset.
            return CharacterWindow[Offset];
        }

        /// <summary>
        /// Gets the character at the given offset to the current position if
        /// the position is valid within the SourceText.
        /// </summary>
        /// <returns>
        /// The next character if any are available. InvalidCharacter otherwise.
        /// </returns>
        public char PeekChar(int delta)
        {
            var position = Position;
            AdvanceChar(delta);

            var ch = Offset >= CharacterWindowCount && !MoreChars() ?
                InvalidCharacter :
                CharacterWindow[Offset];

            Reset(position);
            return ch;
        }

        public void Dispose()
        {
            if (CharacterWindow == null)
                return;

            windowPool.Free(CharacterWindow);
            CharacterWindow = null!;
        }

        private bool MoreChars()
        {
            if (Offset < CharacterWindowCount)
                return true;

            if (Position >= TextEnd)
                return false;

            // if lexeme scanning is sufficiently into the char buffer, 
            // then refocus the window onto the lexeme
            if (LexemeRelativeStart > CharacterWindowCount / 4)
            {
                Array.Copy(CharacterWindow, LexemeRelativeStart, CharacterWindow, 0, CharacterWindowCount - LexemeRelativeStart);
                CharacterWindowCount -= LexemeRelativeStart;
                Offset -= LexemeRelativeStart;
                Basis += LexemeRelativeStart;
                LexemeRelativeStart = 0;
            }

            if (CharacterWindowCount >= CharacterWindow.Length)
            {
                // grow char array, since we need more contiguous space
                var oldWindow = CharacterWindow;
                var newWindow = new char[CharacterWindow.Length * 2];
                Array.Copy(oldWindow, 0, newWindow, 0, CharacterWindowCount);
                CharacterWindow = newWindow;
            }

            var amountToRead = Math.Min(TextEnd - (Basis + CharacterWindowCount), CharacterWindow.Length - CharacterWindowCount);
            Text.CopyTo(Basis + CharacterWindowCount, CharacterWindow, CharacterWindowCount, amountToRead);
            CharacterWindowCount += amountToRead;
            return amountToRead > 0;
        }
    }
}
