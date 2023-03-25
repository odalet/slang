namespace Slang.Utilities
{
    // Copied from Roslyn
    internal static class Extensions
    {
        // same as Array.BinarySearch, but without using IComparer to compare ints
        public static int BinarySearch(this int[] array, int value)
        {
            var low = 0;
            var high = array.Length - 1;

            while (low <= high)
            {
                var middle = low + ((high - low) >> 1);
                var midValue = array[middle];

                if (midValue == value)
                    return middle;

                if (midValue > value)
                    high = middle - 1;
                else
                    low = middle + 1;
            }

            return ~low;
        }
    }
}
