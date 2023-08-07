using System;
using System.Diagnostics;
using System.Threading;

namespace Slang.Utilities;

// Simplified implementation of Roslyn's ObjectPool
internal class ObjectPool<T> where T : class
{
    private struct Element { public T Value; }

    /// <remarks>
    /// Not using System.Func{T} because this file is linked into the (debugger) Formatter,
    /// which does not have that type (since it compiles against .NET 2.0).
    /// </remarks>
    internal delegate T Factory();

    // Storage for the pool objects. The first item is stored in a dedicated field because we
    // expect to be able to satisfy most requests from it.
    private T? firstItem;
    private readonly Element[] otherItems;

    // factory is stored for the lifetime of the pool. We will call this only when pool needs to
    // expand. compared to "new T()", Func gives more flexibility to implementers and faster
    // than "new T()".
    private readonly Factory factory;

    public ObjectPool(Factory factory) : this(factory, Environment.ProcessorCount * 2) { }

    public ObjectPool(Factory objectFactory, int size)
    {
        Debug.Assert(size >= 1);
        factory = objectFactory;
        otherItems = new Element[size - 1];
    }

    /// <summary>
    /// Produces an instance.
    /// </summary>
    /// <remarks>
    /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
    /// Note that Free will try to store recycled objects close to the start thus statistically 
    /// reducing how far we will typically search.
    /// </remarks>
    public T Allocate()
    {
        // PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
        // Note that the initial read is optimistically not synchronized. That is intentional. 
        // We will interlock only when we have a candidate. in a worst case we may miss some
        // recently returned objects. Not a big deal.
        var inst = firstItem;
        if (inst == null || inst != Interlocked.CompareExchange(ref firstItem, null, inst))
            inst = AllocateSlow();

        return inst;
    }

    private T AllocateSlow()
    {
        var items = otherItems;
        for (var i = 0; i < items.Length; i++)
        {
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            var inst = items[i].Value;
            if (inst != null && inst == Interlocked.CompareExchange(ref items[i].Value!, null, inst))
                return inst;
        }

        return CreateInstance();
    }

    private T CreateInstance()
    {
        var inst = factory();
        return inst;
    }

    /// <summary>
    /// Returns objects to the pool.
    /// </summary>
    /// <remarks>
    /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
    /// Note that Free will try to store recycled objects close to the start thus statistically 
    /// reducing how far we will typically search in Allocate.
    /// </remarks>
    internal void Free(T obj)
    {
        if (firstItem == null)
        {
            // Intentionally not using interlocked here. 
            // In a worst case scenario two objects may be stored into same slot.
            // It is very unlikely to happen and will only mean that one of the objects will get collected.
            firstItem = obj;
        }
        else FreeSlow(obj);
    }

    private void FreeSlow(T obj)
    {
        var items = otherItems;
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i].Value == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                items[i].Value = obj;
                break;
            }
        }
    }
}
