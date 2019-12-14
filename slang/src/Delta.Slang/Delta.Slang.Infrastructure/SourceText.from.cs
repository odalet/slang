using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Delta.Slang.Text;

namespace Delta.Slang
{
    partial class SourceText
    {
        /// <summary>
        /// Constructs a <see cref="SourceText"/> from text in a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="encoding">
        /// Encoding of the file that the <paramref name="text"/> was read from or is going to be saved to.
        /// <c>null</c> if the encoding is unspecified.
        /// If the encoding is not specified the resulting <see cref="SourceText"/> isn't debuggable.
        /// If an encoding-less <see cref="SourceText"/> is written to a file a <see cref="Encoding.UTF8"/> shall be used as a default.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="checksumAlgorithm"/> is not supported.</exception>
        public static SourceText From(string text, Encoding encoding = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return new StringText(text, encoding);
        }

        /// <summary>
        /// Constructs a <see cref="SourceText"/> from text in a string.
        /// </summary>
        /// <param name="reader">TextReader</param>
        /// <param name="length">length of content from <paramref name="reader"/></param>
        /// <param name="encoding">
        /// Encoding of the file that the <paramref name="reader"/> was read from or is going to be saved to.
        /// <c>null</c> if the encoding is unspecified.
        /// If the encoding is not specified the resulting <see cref="SourceText"/> isn't debuggable.
        /// If an encoding-less <see cref="SourceText"/> is written to a file a <see cref="Encoding.UTF8"/> shall be used as a default.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="checksumAlgorithm"/> is not supported.</exception>
        public static SourceText From(TextReader reader, Encoding encoding = null)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // This is where distinction between string-based and otherLargeText would appear (if the reader length is > 40Kb)

            var text = reader.ReadToEnd();
            return From(text, encoding);
        }

        /// <summary>
        /// Constructs a <see cref="SourceText"/> from stream content.
        /// </summary>
        /// <param name="stream">Stream. The stream must be seekable.</param>
        /// <param name="encoding">
        /// Data encoding to use if the stream doesn't start with Byte Order Mark specifying the encoding.
        /// <see cref="Encoding.UTF8"/> if not specified.
        /// </param>
        /// <param name="throwIfBinaryDetected">If the decoded text contains at least two consecutive NUL
        /// characters, then an <see cref="InvalidDataException"/> is thrown.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="stream"/> doesn't support reading or seeking.
        /// <paramref name="checksumAlgorithm"/> is not supported.
        /// </exception>
        /// <exception cref="DecoderFallbackException">If the given encoding is set to use a throwing decoder as a fallback</exception>
        /// <exception cref="InvalidDataException">Two consecutive NUL characters were detected in the decoded text and <paramref name="throwIfBinaryDetected"/> was true.</exception>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <remarks>Reads from the beginning of the stream. Leaves the stream open.</remarks>
        public static SourceText From(Stream stream, Encoding encoding = null, bool throwIfBinaryDetected = false)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead || !stream.CanSeek)
                throw new ArgumentException("Stream must support Read and Seek", nameof(stream));

            // This is where distinction between string-based and otherLargeText would appear (if the reader length is > 40Kb)

            var text = Decode(stream, encoding ?? utf8EncodingWithNoBOM, out encoding);
            if (throwIfBinaryDetected && TextUtils.IsBinary(text))
                throw new InvalidDataException();

            return new StringText(text, encoding);
        }

        /// <summary>
        /// Constructs a <see cref="SourceText"/> from a byte array.
        /// </summary>
        /// <param name="buffer">The encoded source buffer.</param>
        /// <param name="length">The number of bytes to read from the buffer.</param>
        /// <param name="encoding">
        /// Data encoding to use if the encoded buffer doesn't start with Byte Order Mark.
        /// <see cref="Encoding.UTF8"/> if not specified.
        /// </param>
        /// <param name="throwIfBinaryDetected">If the decoded text contains at least two consecutive NUL
        /// characters, then an <see cref="InvalidDataException"/> is thrown.</param>
        /// <returns>The decoded text.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="length"/> is negative or longer than the <paramref name="buffer"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="checksumAlgorithm"/> is not supported.</exception>
        /// <exception cref="DecoderFallbackException">If the given encoding is set to use a throwing decoder as a fallback</exception>
        /// <exception cref="InvalidDataException">Two consecutive NUL characters were detected in the decoded text and <paramref name="throwIfBinaryDetected"/> was true.</exception>
        public static SourceText From(byte[] buffer, int length, Encoding encoding = null, bool throwIfBinaryDetected = false)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (length < 0 || length > buffer.Length) throw new ArgumentOutOfRangeException(nameof(length));

            var text = Decode(buffer, length, encoding ?? utf8EncodingWithNoBOM, out encoding);
            if (throwIfBinaryDetected && TextUtils.IsBinary(text))
                throw new InvalidDataException();

            return new StringText(text, encoding);
        }

        /// <summary>
        /// Decodes text from a stream.
        /// </summary>
        /// <param name="stream">The stream containing encoded text.</param>
        /// <param name="encoding">The encoding to use if an encoding cannot be determined from the byte order mark.</param>
        /// <param name="actualEncoding">The actual encoding used.</param>
        /// <returns>The decoded text.</returns>
        /// <exception cref="DecoderFallbackException">If the given encoding is set to use a throwing decoder as a fallback</exception>
        private static string Decode(Stream stream, Encoding encoding, out Encoding actualEncoding)
        {
            Debug.Assert(stream != null);
            Debug.Assert(encoding != null);

            stream.Seek(0, SeekOrigin.Begin);

            var length = (int)stream.Length;
            if (length == 0)
            {
                actualEncoding = encoding;
                return string.Empty;
            }

            // Note: We are setting the buffer size to 4KB instead of the default 1KB. That's
            // because we can reach this code path for FileStreams and, to avoid FileStream
            // buffer allocations for small files, we may intentionally be using a FileStream
            // with a very small (1 byte) buffer. Using 4KB here matches the default buffer
            // size for FileStream and means we'll still be doing file I/O in 4KB chunks.
            using (var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: Math.Min(4096, length), leaveOpen: true))
            {
                var text = reader.ReadToEnd();
                actualEncoding = reader.CurrentEncoding;
                return text;
            }
        }

        /// <summary>
        /// Decodes text from a byte array.
        /// </summary>
        /// <param name="buffer">The byte array containing encoded text.</param>
        /// <param name="length">The count of valid bytes in <paramref name="buffer"/>.</param>
        /// <param name="encoding">The encoding to use if an encoding cannot be determined from the byte order mark.</param>
        /// <param name="actualEncoding">The actual encoding used.</param>
        /// <returns>The decoded text.</returns>
        /// <exception cref="DecoderFallbackException">If the given encoding is set to use a throwing decoder as a fallback</exception>
        private static string Decode(byte[] buffer, int length, Encoding encoding, out Encoding actualEncoding)
        {
            Debug.Assert(buffer != null);
            Debug.Assert(encoding != null);

            actualEncoding = TextUtils.TryReadByteOrderMark(buffer, length, out int preambleLength) ?? encoding;
            return actualEncoding.GetString(buffer, preambleLength, length - preambleLength);
        }
    }
}
