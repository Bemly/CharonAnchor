using Lagrange.Milky.Extension;

namespace Lagrange.Milky.Utility.Cache;

public sealed class FifoCache<TKey, TValue>(int capacity) : ICache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity = capacity;
    private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _cache = [];
    private readonly LinkedList<KeyValuePair<TKey, TValue>> _sorted = [];

    private readonly ReaderWriterLockSlim _lock = new();

    public TValue? Get(TKey key)
    {
        using (_lock.UsingReadLock())
        {
            if (!_cache.TryGetValue(key, out LinkedListNode<KeyValuePair<TKey, TValue>>? node)) return default;

            return node.Value.Value;
        }
    }

    public void Put(TKey key, TValue value)
    {
        using (_lock.UsingWriteLock())
        {
            if (_cache.TryGetValue(key, out LinkedListNode<KeyValuePair<TKey, TValue>>? node))
            {
                _sorted.Remove(node);
                _sorted.AddFirst(new LinkedListNode<KeyValuePair<TKey, TValue>>(new(key, value)));
            }
            else
            {
                if (_cache.Count == _capacity) _sorted.RemoveLast();

                KeyValuePair<TKey, TValue> item = new(key, value);
                node = _sorted.AddFirst(item);
                _cache.Add(key, node);
            }
        }
    }
}