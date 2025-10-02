using System.Net;
using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public class RemoteSource : BaseSource
{
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
    var myTasks = slices.Select(slice => this.fetchSlice(slice, signal));
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
      var contentType = response.Headers.GetValues("content-type").First();
      
       
      // const { type, params } = parseContentType(response.getHeader('content-type'));
      if (contentType == "multipart/byteranges") {
        throw new NotImplementedException();
        // var byteRanges = parseByteRanges(await response.getData(), params.boundary);
        // this._fileSize = byteRanges[0].fileSize || null;
        // return byteRanges;
      }
      
      var data = await response.getData();
      
      const { start, end, total } = parseContentRange(response.getHeader('content-range'));
      this._fileSize = total || null;
      const first = [{
        data,
        offset: start,
        length: end - start,
      }];
      
      if (slices.length > 1) {
        // we requested more than one slice, but got only the first
        // unfortunately, some HTTP Servers don't support multi-ranges
        // and return only the first
      
        // get the rest of the slices and fetch them iteratively
        const others = await JSType.Promise<>.all(slices.slice(1).map((slice) => this.fetchSlice(slice, signal)));
        return first.concat(others);
      }
      return first;
    } else {
      if (!this.allowFullFile) {
        throw new GeotiffNetworkException("Server responded with full file. If this is intentional behaviour, call RemoteSource with allowFullFile = true");
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
      var ab = new ArrayBuffer(ms.ToArray());
      this._fileSize = ms.Length;
      return new[] { ab };
      // return [{
      //   data,
      //   offset: 0,
      //   length: data.byteLength,
      // }];
    }
  }

  public async Task<ArrayBuffer> fetchSlice(Slice slice, CancellationToken? signal = null)
  {
    var offset = slice.offset;
    var length = slice.length;
    using HttpRequestMessage request = new(
      HttpMethod.Get,
      this.url);
    request.Headers.Add("Range", $"{slice.offset}-${slice.offset + slice.length}");
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
      throw new NotImplementedException("Multi range not implemented yet");
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

  public long? fileSize => this._fileSize;
}