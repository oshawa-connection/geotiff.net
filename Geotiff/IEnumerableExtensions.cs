namespace Geotiff;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> UnboxAll<T>(this IEnumerable<object> list)
    {
        return list.Select(d => (T)d);
    }
}