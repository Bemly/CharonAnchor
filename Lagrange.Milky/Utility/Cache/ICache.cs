using System.Diagnostics.CodeAnalysis;

namespace Lagrange.Milky.Utility.Cache;

public interface ICache<TKey, TValue>
{
    TValue? Get(TKey key);

    void Put(TKey key, TValue value);
}