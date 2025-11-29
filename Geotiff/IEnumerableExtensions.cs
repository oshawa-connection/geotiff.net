namespace Geotiff;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> UnboxAll<T>(this IEnumerable<object> list)
    {
        Type? targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        return list.Select(d => (T)Convert.ChangeType(d, targetType));
    }
}