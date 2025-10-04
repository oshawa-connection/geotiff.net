using Geotiff.JavaScriptCompatibility;

namespace Geotiff.RemoteClients;

public interface IGeotiffRemoteClient 
{
    public Task<IEnumerable<ArrayBuffer>> FetchSlices(IEnumerable<Slice> slices, CancellationToken? signal = null);
    public Task<ArrayBuffer> FetchSlice(Slice slice, CancellationToken? signal = null);
}