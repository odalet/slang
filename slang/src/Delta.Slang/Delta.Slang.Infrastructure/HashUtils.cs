using System.Runtime.CompilerServices;

namespace Delta.Slang
{
    // Copied from Roslyn
    internal static class HashUtils
    {
        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Combine(int left, int right) => unchecked((right * (int)0xA5555529) + left);

        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// PERF: Do not use with enum types because that involves multiple
        /// unnecessary boxing operations.  Unfortunately, we can't constrain
        /// T to "non-enum", so we'll use a more restrictive constraint.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Combine<T>(T left, int right) where T : class
        {
            var hash = unchecked(right * (int)0xA5555529);
            if (left != null)
                return unchecked(hash + left.GetHashCode());
            return hash;
        }
    }
}
