using System.Collections;

namespace Geotiff;

public class BiDirectionalDictionary<TKey, TValue> : IEnumerable where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _forward = new();
    private readonly Dictionary<TValue, TKey> _reverse = new();

    public void Add(TKey key, TValue value)
    {
        _forward.Add(key, value);
        _reverse.Add(value, key);
    }

    public TValue GetByKey(TKey key)
    {
        return _forward[key];
    }

    public TKey GetByValue(TValue value)
    {
        return _reverse[value];
    }

    public bool TryGetByKey(TKey key, out TValue value)
    {
        return _forward.TryGetValue(key, out value);
    }

    public bool TryGetByValue(TValue value, out TKey key)
    {
        return _reverse.TryGetValue(value, out key);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotSupportedException();
    }
}