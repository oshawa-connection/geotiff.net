using System.Net;
using System.Runtime.InteropServices.JavaScript;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public class RemoteSource : BaseSource {
  
  private int maxRanges { get; set; }
  private HttpClient client { get; set; }
  private bool allowFullFile { get; set; }
  private int? _fileSize { get; set; }
  /**
   * @param {BaseClient} client
   * @param {object} headers
   * @param {numbers} maxRanges
   * @param {boolean} allowFullFile
   */
  RemoteSource(HttpClient client, int maxRanges, bool allowFullFile) {
    this.client = client;
    // this.headers = headers; // Let user do this on httpclient itself.
    this.maxRanges = maxRanges;
    this.allowFullFile = allowFullFile;
    this._fileSize = null;
  }

  
  // public async Task<IEnumerable<ArrayBuffer>> Fetch(IEnumerable<Slice> slices, CancellationToken? cancellationToken)
  /**
   *
   * @param {Slice[]} slices
   */
  public async Task<IEnumerable<ArrayBuffer>> Fetch(IEnumerable<Slice> slices, CancellationToken signal) {
    // if we allow multi-ranges, split the incoming request into that many sub-requests
    // and join them afterwards
    if (this.maxRanges >= slices.Count()) {
      return this.fetchSlices(slices, signal);
    } else if (this.maxRanges > 0 && slices.Count() > 1) {
      // TODO: split into multiple multi-range requests

      // const subSlicesRequests = [];
      // for (let i = 0; i < slices.length; i += this.maxRanges) {
      //   subSlicesRequests.push(
      //     this.fetchSlices(slices.slice(i, i + this.maxRanges), signal),
      //   );
      // }
      // return (await Promise.all(subSlicesRequests)).flat();
    }

    // otherwise make a single request for each slice
    return JSType.Promise<int>.All(
      slices.map((slice) => this.fetchSlice(slice, signal)),
    );
  }

  private async Task<IEnumerable<ArrayBuffer>> fetchSlices(IEnumerable<Slice> slices, CancellationToken signal) {
    using HttpRequestMessage request = new(
      HttpMethod.Head, 
      "https://www.example.com");
    var rangeHeaderValue = String.Join(',', slices
      .Select((d) => $"{d.offset}-${d.offset + d.length}"));
    
    request.Headers.Add("Range", rangeHeaderValue);
    var response = await this.client.SendAsync(request, signal);


    if (!response.ok) {
      throw new JSType.Error('Error fetching data.');
    } else if (response.status === 206) {
      const { type, params } = parseContentType(response.getHeader('content-type'));
      if (type === 'multipart/byteranges') {
        const byteRanges = parseByteRanges(await response.getData(), params.boundary);
        this._fileSize = byteRanges[0].fileSize || null;
        return byteRanges;
      }

      const data = await response.getData();

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
        throw new JSType.Error('Server responded with full file');
      }
      const data = await response.getData();
      this._fileSize = data.byteLength;
      return [{
        data,
        offset: 0,
        length: data.byteLength,
      }];
    }
  }

  async fetchSlice(slice, signal) {
    const { offset, length } = slice;
    const response = await this.client.request({
      headers: {
        ...this.headers,
        Range: `bytes=${offset}-${offset + length}`,
      },
      signal,
    });

    // check the response was okay and if the server actually understands range requests
    if (!response.ok) {
      throw new JSType.Error('Error fetching data.');
    } else if (response.status === 206) {
      const data = await response.getData();

      const { total } = parseContentRange(response.getHeader('content-range'));
      this._fileSize = total || null;
      return {
        data,
        offset,
        length,
      };
    } else {
      if (!this.allowFullFile) {
        throw new JSType.Error('Server responded with full file');
      }

      const data = await response.getData();

      this._fileSize = data.byteLength;
      return {
        data,
        offset: 0,
        length: data.byteLength,
      };
    }
  }

  get fileSize() {
    return this._fileSize;
  }
}