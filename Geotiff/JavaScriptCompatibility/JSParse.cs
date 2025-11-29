namespace Geotiff.JavaScriptCompatibility;

internal static class JsParse
{
    public static int ParseInt(string s, int radix)
    {
        if (radix < 2 || radix > 36)
        {
            throw new ArgumentOutOfRangeException(nameof(radix), "Radix must be between 2 and 36.");
        }

        int result = 0;

        foreach (char c in s)
        {
            int digit;
            if (c >= '0' && c <= '9')
            {
                digit = c - '0';
            }
            else if (c >= 'a' && c <= 'z')
            {
                digit = c - 'a' + 10;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                digit = c - 'A' + 10;
            }
            else
            {
                break; // stop on invalid char
            }

            if (digit >= radix)
            {
                break; // invalid digit for this radix
            }

            result = (result * radix) + digit;
        }

        return result;
    }
}