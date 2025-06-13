using Lagrange.Milky.Extension;

namespace Lagrange.Milky.Utility.Cache;

public sealed class LruCache<TKey, TValue>(int capacity) : ICache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity = capacity;
    private readonly Dictionary<TKey, LinkedListNode<LruCacheNode>> _cache = [];
    private readonly LinkedList<LruCacheNode> _sorted = [];

    private readonly ReaderWriterLockSlim _lock = new();

    public TValue? Get(TKey key)
    {
        using (_lock.UsingUpgradeableReadLock())
        {
            if (!_cache.TryGetValue(key, out LinkedListNode<LruCacheNode>? node)) return default;

            using (_lock.UsingWriteLock())
            {
                _sorted.Remove(node);
                _sorted.AddFirst(node);

                return node.Value.Value;
            }
        }
    }

    public void Put(TKey key, TValue value)
    {
        using (_lock.UsingWriteLock())
        {
            if (_cache.TryGetValue(key, out LinkedListNode<LruCacheNode>? node))
            {
                node.Value.Value = value;
                _sorted.Remove(node);
                _sorted.AddFirst(node);
            }
            else
            {
                if (_cache.Count == _capacity) _sorted.RemoveLast();

                LruCacheNode item = new(key, value);
                node = _sorted.AddFirst(item);
                _cache.Add(key, node);
            }
        }
    }

    private sealed class LruCacheNode(TKey key, TValue value)
    {
        public TKey Key { get; } = key;

        public TValue Value { get; set; } = value;
    }
}