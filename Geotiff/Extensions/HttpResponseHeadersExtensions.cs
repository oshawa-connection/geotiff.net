using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Geotiff.Extensions;

public static class HttpResponseHeadersExtensions
{
    /// <summary>
    /// Parses the 'Content-Range' header value into start, end, and total.
    /// Returns null if the header is missing or invalid.
    /// </summary>
    /// <param name="headers">The HttpResponseHeaders instance.</param>
    /// <returns>
    /// A tuple (start, end, total) if parsing succeeds, otherwise null.
    /// </returns>
    public static ContentRangeHeaderParseResult? ParseContentRange(this HttpResponseHeaders headers)
    {
        if (headers == null)
        {
            throw new ArgumentNullException(nameof(headers));
        }

        if (!headers.TryGetValues("Content-Range", out IEnumerable<string>? values))
        {
            return null;
        }

        string? rawContentRange = string.Join(",", values); // In practice, only one value is expected
        // Example: bytes 0-99/200
        Match? match = Regex.Match(rawContentRange, @"bytes (\d+)-(\d+)/(\d+)");
        if (match.Success)
        {
            long start = long.Parse(match.Groups[1].Value);
            long end = long.Parse(match.Groups[2].Value);
            long total = long.Parse(match.Groups[3].Value);
            return new ContentRangeHeaderParseResult() { end = end, start = start, total = total };
        }

        return null;
    }


    public class ByteRangePart
    {
        public Dictionary<string, string> Headers { get; set; }
        public byte[] Data { get; set; }
        public long Offset { get; set; }
        public long Length { get; set; }
        public long FileSize { get; set; }
    }

    // Parse HTTP headers from a given string.
    private static Dictionary<string, string> ParseHeaders(string text)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[]? lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
        foreach (string? line in lines)
        {
            string[]? kv = line.Split(new[] { ':' }, 2);
            if (kv.Length == 2)
            {
                headers[kv[0].Trim().ToLower()] = kv[1].Trim();
            }
        }

        return headers;
    }
}

public class ContentRangeHeaderParseResult
{
    public long start { get; set; }
    public long end { get; set; }
    public long total { get; set; }
}