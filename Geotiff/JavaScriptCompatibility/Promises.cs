namespace Geotiff.JavaScriptCompatibility;

public static class Promises
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="then"></param>
    /// <typeparam name="T">input type</typeparam>
    /// <typeparam name="TV">Return type</typeparam>
    /// <returns></returns>
    public static async Task<TV> Then<T,TV>(this Task<T> task, Func<T,TV> then)
    {
        var result = await task;
        return then(result);
    }
}