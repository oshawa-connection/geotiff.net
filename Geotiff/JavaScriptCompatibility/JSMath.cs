namespace Geotiff.JavaScriptCompatibility;
/// <summary>
/// JS like Math functions from https://stackoverflow.com/a/6800845
/// </summary>
public static class JSMath
{
    // This method only exists for consistency, so you can *always* call
    // MoreMath.Max instead of alternating between MoreMath.Max and Math.Max
    // depending on your argument count.
    public static int Max(int x, int y)
    {
        return Math.Max(x, y);
    }

    public static int Max(int x, int y, int z)
    {
        // Or inline it as x < y ? (y < z ? z : y) : (x < z ? z : x);
        // Time it before micro-optimizing though!
        return Math.Max(x, Math.Max(y, z));
    }

    public static int Max(int w, int x, int y, int z)
    {
        return Math.Max(w, Math.Max(x, Math.Max(y, z)));
    }

    public static int Max(params int[] values)
    {
        return Enumerable.Max(values);
    }
    
    // public static int Min(params int[] values)
    // {
    //     return Enumerable.Min(values);
    // }
    
    public static T Min<T>(params T[] values)
    {
        return Enumerable.Min(values);
    }
    
    
    // public static ulong Min(params ulong[] values)
    // {
    //     return Enumerable.Min(values);
    // }
}
