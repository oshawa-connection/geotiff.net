using Geotiff.JavaScriptCompatibility;

namespace Geotiff.RemoteClients;

/// <summary>
/// Public to allow others to implement their own.
/// </summary>
public interface IGeotiffRemoteClient
{
    Task<IEnumerable<ArrayBuffer>> FetchSlicesAsync(IEnumerable<Slice> slices, CancellationToken? signal = null);
    Task<ArrayBuffer> FetchSliceAsync(Slice slice, CancellationToken? signal = null);
    
    
    IEnumerable<ArrayBuffer> FetchSlices(IEnumerable<Slice> slices);
    ArrayBuffer FetchSlice(Slice slice);
}