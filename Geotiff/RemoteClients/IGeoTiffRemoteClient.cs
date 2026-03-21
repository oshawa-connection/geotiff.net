using Geotiff.JavaScriptCompatibility;

namespace Geotiff.RemoteClients;

/// <summary>
/// Public to allow others to implement their own.
/// </summary>
public interface IGeoTiffRemoteClient
{
    Task<IEnumerable<byte[]>> FetchSlicesAsync(IEnumerable<Slice> slices, CancellationToken? signal = null);
    Task<byte[]> FetchSliceAsync(Slice slice, CancellationToken? signal = null);
    
    
    IEnumerable<byte[]> FetchSlices(IEnumerable<Slice> slices);
    byte[] FetchSlice(Slice slice);
}