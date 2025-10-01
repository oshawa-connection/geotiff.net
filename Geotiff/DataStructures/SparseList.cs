namespace Geotiff;

using System.Collections.Generic;

public class SparseList<T>
{
    private readonly Dictionary<int, T> _dict = new();

    public void Add(int index, T value)
    {
        _dict[index] = value;
    }

    public T this[int index]
    {
        get => _dict.TryGetValue(index, out var value) ? value : default;
        set => _dict[index] = value;
    }
}