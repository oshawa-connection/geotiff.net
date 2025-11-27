using System.Net;
using System.Text;
using Geotiff.Exceptions;
using Geotiff.Extensions;
using Geotiff.JavaScriptCompatibility;
using Geotiff.RemoteClients;

namespace Geotiff;

public class RemoteSource : BaseSource
{
  private const string CRLFCRLF = "\r\n\r\n";
  public long? fileSize => this._fileSize;
  
  private readonly string url;
  private int maxRanges { get; set; }
  private IGeotiffRemoteClient client { get; set; }
  private bool allowFullFile { get; set; }
  private long? _fileSize { get; set; }
  /**
   * @param {BaseClient} client
   * @param {object} headers
   * @param {numbers} maxRanges
   * @param {boolean} allowFullFile
   */
  public RemoteSource(IGeotiffRemoteClient client, int maxRanges, bool allowFullFile)
  {
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

  public async Task<IEnumerable<ArrayBuffer>> FetchSlices(IEnumerable<Slice> slices, CancellationToken? signal = null)
  {
    return await this.client.FetchSlices(slices, signal);
  }

  public override async Task<ArrayBuffer> FetchSlice(Slice slice, CancellationToken? signal = null)
  {
    return await this.client.FetchSlice(slice, signal);
  }
}