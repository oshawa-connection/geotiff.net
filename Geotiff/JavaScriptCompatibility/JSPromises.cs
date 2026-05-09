namespace Geotiff.JavaScriptCompatibility;

internal static class JSPromises
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="then"></param>
    /// <typeparam name="T">input type</typeparam>
    /// <typeparam name="TV">Return type</typeparam>
    /// <returns></returns>
    public static async Task<TV> JSThen<T, TV>(this Task<T> task, int i, Func<T, int, TV> then)
    {
        T? result = await task;
        return then(result, i);
    }
}