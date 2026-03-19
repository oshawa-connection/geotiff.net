namespace Geotiff.JavaScriptCompatibility;

/// <summary>
/// JS like Math functions from https://stackoverflow.com/a/6800845
/// </summary>
internal static class JSMath
{
    public static T Min<T>(params T[] values)
    {
        return Enumerable.Min(values);
    }
}