namespace Geotiff.JavaScriptCompatibility;

public static class StringExtensions
{
    public static string JSSubString(this string str, int startIndex, int endIndex)
    {
        if (str == null)
        {
            return null;
        }

        // Clamp indices
        int len = str.Length;
        startIndex = Math.Max(0, Math.Min(startIndex, len));
        endIndex = Math.Max(0, Math.Min(endIndex, len));

        // Swap if startIndex > endIndex
        if (startIndex > endIndex)
        {
            int temp = startIndex;
            startIndex = endIndex;
            endIndex = temp;
        }

        return str.Substring(startIndex, endIndex - startIndex);
    }
}