using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Delta.Slang.Pooling
{
    // Fake implementation of Roslyn's ArrayBuilder
    internal sealed class ArrayBuilder<T> : IReadOnlyList<T>
    {
        private readonly List<T> collection;

        public ArrayBuilder(int size) => collection = new List<T>(size);
        public ArrayBuilder() : this(8) { }

        public int Count => collection.Count;

        public T this[int index] => collection[index];

        public static ArrayBuilder<T> GetInstance() => new ArrayBuilder<T>();

        public void Add(T item) => collection.Add(item);

        public T[] ToArrayAndFree()
        {
            var result = collection.ToArray();
            collection.Clear();
            return result;
        }

        public ImmutableArray<T> ToImmutable() => ImmutableArray.CreateRange(collection);

        public IEnumerator<T> GetEnumerator() => collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
