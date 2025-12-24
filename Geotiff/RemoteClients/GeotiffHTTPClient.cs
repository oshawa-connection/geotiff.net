using System.Net;
using Geotiff.Exceptions;
using Geotiff.Extensions;
using Geotiff.JavaScriptCompatibility;
using System.Text.RegularExpressions;

namespace Geotiff.RemoteClients;

public class GeotiffHTTPClient : IGeotiffRemoteClient
{
    private string url { get; set; }
    private HttpClient client { get; set; }
    private bool allowFullFile { get; set; }

    public GeotiffHTTPClient(string url, HttpClient client, bool allowFullFile)
    {
        this.url = url;
        this.client = client;
        this.allowFullFile = allowFullFile;
    }

    public async Task<IEnumerable<ArrayBuffer>> FetchSlicesAsync(IEnumerable<Slice> slices, CancellationToken? signal = null)
    {
        using HttpRequestMessage request = new(
            HttpMethod.Get,
            url);
        string? rangeHeaderValue = string.Join(',', slices
            .Select((d) => $"{d.Offset}-{d.Offset + d.Length}"));

        request.Headers.Add("Range", $"bytes={rangeHeaderValue}");
        HttpResponseMessage? response;
        if (signal != null)
        {
            response = await client.SendAsync(request, (CancellationToken)signal);
        }
        else
        {
            response = await client.SendAsync(request);
        }

        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.PartialContent)
        {
            bool found = response.Headers.TryGetValues("Content-Type", out IEnumerable<string>? x);

            // const { type, params } = parseContentType(response.getHeader('content-type'));
            if (found is true && x.First() == "multipart/byteranges")
            {
                var ms = new MemoryStream();
                await response.Content.CopyToAsync(ms);
                ms.Position = 0;

                IEnumerable<ArrayBuffer>? byteRanges = ParseByteRanges(ms.ToArray(), "params.boundary");
                // this._fileSize = byteRanges.First().Length != 0 ? byteRanges.First().Length : null;
                return byteRanges;
            }

            ArrayBuffer? data = await ArrayBuffer.FromStreamAsync(await response.Content.ReadAsStreamAsync(), signal);

            ContentRangeHeaderParseResult? contentRangeResult = response.Headers.ParseContentRange();
            if (contentRangeResult is not null)
            {
                // this._fileSize = contentRangeResult.total == 0 ? contentRangeResult.total : null;  
            }

            if (slices.Count() > 1)
            {
                throw new NotImplementedException("Server returned an invalid response to a multi-slice request");
                // we requested more than one slice, but got only the first
                // unfortunately, some HTTP Servers don't support multi-ranges
                // and return only the first

                // get the rest of the slices and fetch them iteratively
                // const others = await JSType.Promise<>.all(slices.slice(1).map((slice) => this.fetchSlice(slice, signal)));
                // return first.concat(others);
            }

            return new List<ArrayBuffer>() { data };
        }
        else
        {
            if (!allowFullFile)
            {
                throw new GeotiffNetworkException(
                    "Server responded with full file. If this is intentional behaviour, call RemoteSource with allowFullFile = true");
            }

            ArrayBuffer? data = await ArrayBuffer.FromStreamAsync(await response.Content.ReadAsStreamAsync(), signal);

            // this._fileSize = data.Length;
            return new[] { data };
        }
    }

    public async Task<ArrayBuffer> FetchSliceAsync(Slice slice, CancellationToken? signal = null)
    {
        int offset = slice.Offset;
        int length = slice.Length;
        using HttpRequestMessage request = new(
            HttpMethod.Get,
            url);
        request.Headers.Add("Range", $"bytes={slice.Offset}-{slice.Offset + slice.Length}");
        HttpResponseMessage? response;

        if (signal != null)
        {
            response = await client.SendAsync(request, (CancellationToken)signal);
        }
        else
        {
            response = await client.SendAsync(request);
        }


        // check the response was okay and if the server actually understands range requests
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == HttpStatusCode.PartialContent)
        {
            // TODO: Handle parseContentRange here
            ArrayBuffer? data = await ArrayBuffer.FromStreamAsync(await response.Content.ReadAsStreamAsync(), signal);
            return data;
            //   const data = await response.getData();
            //
            //   const { total } = parseContentRange(response.getHeader('content-range'));
            //   this._fileSize = total || null;
            //   return {
            //     data,
            //     offset,
            //     length,
            //   };
        }
        else
        {
            if (!allowFullFile)
            {
                throw new GeotiffNetworkException("Server responded with full file");
            }

            var ms = new MemoryStream();
            if (signal != null)
            {
                await response.Content.CopyToAsync(ms); // CancellationToken not available here in netstadnard2.1
            }
            else
            {
                await response.Content.CopyToAsync(ms);
            }

            ms.Position = 0;

            // this._fileSize = ms.Length;
            return new ArrayBuffer(ms.ToArray());

            // this._fileSize = data.byteLength;
            // return {
            //   data,
            //   offset: 0,
            //   length: data.byteLength,
            // };
        }
    }

    public IEnumerable<ArrayBuffer> FetchSlices(IEnumerable<Slice> slices)
    {
        throw new NotImplementedException();
    }

    public ArrayBuffer FetchSlice(Slice slice)
    {
        throw new NotImplementedException();
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


    private static IEnumerable<ArrayBuffer> ParseByteRanges(byte[] responseBytes, string boundary)
    {
        var parts = new List<ArrayBuffer>();
        string? boundaryString = $"--{boundary}";
        string? endBoundaryString = $"{boundaryString}--";
        string? responseText = System.Text.Encoding.ASCII.GetString(responseBytes);

        string[]? sections = responseText.Split(new[] { boundaryString }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string? section in sections)
        {
            if (section.StartsWith("--")) // end boundary
            {
                continue;
            }

            int headerBodySplit = section.IndexOf("\r\n\r\n");
            if (headerBodySplit < 0)
            {
                continue;
            }

            string? headerText = section.Substring(0, headerBodySplit).Trim();
            string? bodyText = section.Substring(headerBodySplit + 4);

            // Parse headers
            Dictionary<string, string>? headers = headerText
                .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(new[] { ':' }, 2))
                .ToDictionary(kv => kv[0].Trim().ToLower(), kv => kv[1].Trim());

            // Parse Content-Range
            string? contentRange = headers["content-range"];
            Match? match = System.Text.RegularExpressions.Regex.Match(contentRange, @"bytes (\d+)-(\d+)/(\d+)");
            long start = long.Parse(match.Groups[1].Value);
            long end = long.Parse(match.Groups[2].Value);
            long total = long.Parse(match.Groups[3].Value);

            // Extract data bytes
            byte[]? bodyBytes = System.Text.Encoding.ASCII.GetBytes(bodyText);
            long length = end - start + 1;

            parts.Add(new ArrayBuffer(bodyBytes));
        }

        return parts;
    }
}