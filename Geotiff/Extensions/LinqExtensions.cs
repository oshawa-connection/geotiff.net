using System;
using System.Collections.Generic;
using System.Linq;

namespace Geotiff.Extensions;

/// <summary>
/// Credit: https://stackoverflow.com/a/65459077
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// Skipping more than int32 bytes when tile/ strip offsets are larger than Int32.MaxValue
    /// </summary>
    /// <param name="items"></param>
    /// <param name="howMany"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> BigSkip<T>(this IEnumerable<T> items, long howMany)
        => BigSkip(items, Int32.MaxValue, howMany);

    internal static IEnumerable<T> BigSkip<T>(this IEnumerable<T> items, int segmentSize, long howMany)
    {
        long segmentCount = Math.DivRem(howMany, segmentSize,
            out long remainder);

        for (long i = 0; i < segmentCount; i += 1)
            items = items.Skip(segmentSize);

        if (remainder != 0)
            items = items.Skip((int)remainder);

        return items;
    }
}