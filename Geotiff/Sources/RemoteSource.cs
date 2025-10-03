using System.Net;
using System.Text;
using Geotiff.Exceptions;
using Geotiff.Extensions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public class RemoteSource : BaseSource
{
  private const string CRLFCRLF = "\r\n\r\n";
  public long? fileSize => this._fileSize;
  
  private readonly string url;
  private int maxRanges { get; set; }
  private HttpClient client { get; set; }
  private bool allowFullFile { get; set; }
  private long? _fileSize { get; set; }
  /**
   * @param {BaseClient} client
   * @param {object} headers
   * @param {numbers} maxRanges
   * @param {boolean} allowFullFile
   */
  public RemoteSource(string url, HttpClient client, int maxRanges, bool allowFullFile)
  {
    this.url = url;
    this.client = client;
    // this.headers = headers; // Let user do this on httpclient itself.
    this.maxRanges = maxRanges;
    this.allowFullFile = allowFullFile;
    this._fileSize = null;
  }
  
  /**
   *
   * @param {Slice[]} slices
   */
  public async Task<IEnumerable<ArrayBuffer>> Fetch(IEnumerable<Slice> slices, CancellationToken? signal = null) {
    // if we allow multi-ranges, split the incoming request into that many sub-requests
    // and join them afterwards
    if (this.maxRanges >= slices.Count()) {
      return await this.FetchSlices(slices, signal);
    } else if (this.maxRanges > 0 && slices.Count() > 1) {
      // TODO: split into multiple multi-range requests - comment was in geotiff.js

      // const subSlicesRequests = [];
      // for (let i = 0; i < slices.length; i += this.maxRanges) {
      //   subSlicesRequests.push(
      //     this.fetchSlices(slices.slice(i, i + this.maxRanges), signal),
      //   );
      // }
      // return (await Promise.all(subSlicesRequests)).flat();
    }
    
    // otherwise make a single request for each slice
    var myTasks = slices.Select(slice => this.FetchSlice(slice, signal));
    return await Task.WhenAll(myTasks);
  }

  public async Task<IEnumerable<ArrayBuffer>> FetchSlices(IEnumerable<Slice> slices, CancellationToken? signal = null) {
    using HttpRequestMessage request = new(
      HttpMethod.Get,
      this.url);
    var rangeHeaderValue = String.Join(',', slices
      .Select((d) => $"{d.offset}-{d.offset + d.length}"));
    
    request.Headers.Add("Range", $"bytes={rangeHeaderValue}");
    HttpResponseMessage? response;
    if (signal != null)
    {
      response = await this.client.SendAsync(request, (CancellationToken) signal);
    }
    else
    {
      response = await this.client.SendAsync(request);
    }

    response.EnsureSuccessStatusCode();

    if (response.StatusCode == HttpStatusCode.PartialContent)
    {
      
      var found = response.Headers.TryGetValues("Content-Type", out var x);
      
      // const { type, params } = parseContentType(response.getHeader('content-type'));
      if (found is true && x.First() == "multipart/byteranges")
      {
        var ms = new MemoryStream();
        await response.Content.CopyToAsync(ms);
        ms.Position = 0;
        
        var byteRanges = ParseByteRanges(ms.ToArray(), "params.boundary");
        this._fileSize = byteRanges.First().Length != 0 ? byteRanges.First().Length : null;
        return byteRanges;
      }

      var data = await ArrayBuffer.FromStream(response.Content.ReadAsStream(), signal);
      
      var contentRangeResult = response.Headers.ParseContentRange();
      if (contentRangeResult is not null)
      {
        this._fileSize = contentRangeResult.total == 0 ? contentRangeResult.total : null;  
      }
      

      
      // const first = [{
      //   data,
      //   offset: start,
      //   length: end - start,
      // }];
      
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
    } else {
      if (!this.allowFullFile) {
        throw new GeotiffNetworkException("Server responded with full file. If this is intentional behaviour, call RemoteSource with allowFullFile = true");
      }
  
      var data = await ArrayBuffer.FromStream(response.Content.ReadAsStream(), signal);
      
      this._fileSize = data.Length;
      return new[] { data };
      // return [{
      //   data,
      //   offset: 0,
      //   length: data.byteLength,
      // }];
    }
  }

  public override async Task<ArrayBuffer> FetchSlice(Slice slice, CancellationToken? signal = null)
  {
    var offset = slice.offset;
    var length = slice.length;
    using HttpRequestMessage request = new(
      HttpMethod.Get,
      this.url);
    request.Headers.Add("Range", $"bytes={slice.offset}-{slice.offset + slice.length}");
    HttpResponseMessage? response;

    if (signal != null)
    {
      response = await this.client.SendAsync(request, (CancellationToken)signal);
    }
    else
    {
      response = await this.client.SendAsync(request);
    }
    
    
    // check the response was okay and if the server actually understands range requests
    response.EnsureSuccessStatusCode();
    if (response.StatusCode == HttpStatusCode.PartialContent)
    {
      // TODO: Handle parseContentRange here
      var data = await ArrayBuffer.FromStream(await response.Content.ReadAsStreamAsync(),signal);
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
    } else {
        if (!this.allowFullFile) {
          throw new GeotiffNetworkException("Server responded with full file");
        }
      
        var ms = new MemoryStream();
        if (signal != null)
        {
          await response.Content.CopyToAsync(ms, (CancellationToken)signal);  
        }
        else
        {
          await response.Content.CopyToAsync(ms);
        }
        
        ms.Position = 0;
        
        this._fileSize = ms.Length;
        return new ArrayBuffer(ms.ToArray());
      
        // this._fileSize = data.byteLength;
        // return {
        //   data,
        //   offset: 0,
        //   length: data.byteLength,
        // };
    }
  }
  

  // Parse HTTP headers from a given string.
  private static Dictionary<string, string> ParseHeaders(string text)
  {
    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
    foreach (var line in lines)
    {
      var kv = line.Split(new[] { ':' }, 2);
      if (kv.Length == 2)
      {
        headers[kv[0].Trim().ToLower()] = kv[1].Trim();
      }
    }
    return headers;
  }
  
  
  public static IEnumerable<ArrayBuffer> ParseByteRanges(byte[] responseBytes, string boundary)
  {
    var parts = new List<ArrayBuffer>();
    var boundaryString = $"--{boundary}";
    var endBoundaryString = $"{boundaryString}--";
    var responseText = System.Text.Encoding.ASCII.GetString(responseBytes);

    var sections = responseText.Split(new[] { boundaryString }, StringSplitOptions.RemoveEmptyEntries);

    foreach (var section in sections)
    {
      if (section.StartsWith("--")) // end boundary
        continue;

      var headerBodySplit = section.IndexOf("\r\n\r\n");
      if (headerBodySplit < 0)
        continue;

      var headerText = section.Substring(0, headerBodySplit).Trim();
      var bodyText = section.Substring(headerBodySplit + 4);

      // Parse headers
      var headers = headerText
        .Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.Split(new[] { ':' }, 2))
        .ToDictionary(kv => kv[0].Trim().ToLower(), kv => kv[1].Trim());

      // Parse Content-Range
      var contentRange = headers["content-range"];
      var match = System.Text.RegularExpressions.Regex.Match(contentRange, @"bytes (\d+)-(\d+)/(\d+)");
      var start = long.Parse(match.Groups[1].Value);
      var end = long.Parse(match.Groups[2].Value);
      var total = long.Parse(match.Groups[3].Value);

      // Extract data bytes
      var bodyBytes = System.Text.Encoding.ASCII.GetBytes(bodyText);
      var length = end - start + 1;
      
      parts.Add(new ArrayBuffer(bodyBytes));
    }

    return parts;
  }
}