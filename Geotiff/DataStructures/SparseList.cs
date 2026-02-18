using System.Collections;

namespace Geotiff;

using System.Collections.Generic;

public class SparseList<T> : IEnumerable<T>
{
    private readonly Dictionary<int, T> _dict = new();

    public void Add(int index, T value)
    {
        _dict[index] = value;
    }

    public T this[int index]
    {
        get => _dict.TryGetValue(index, out T? value) ? value : default;
        set => _dict[index] = value;
    }

    public IEnumerable<int> GetIndices()
    {
        return this._dict.Keys;
    }

    /// <summary>
    /// For users who might need to pass this on somewhere
    /// </summary>
    /// <returns></returns>
    public List<T> ToList()
    {
        return _dict.Values.ToList();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this._dict.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}