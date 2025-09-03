using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

public abstract class GeotiffDecoder
{
    public abstract IEnumerable<int> codes { get; }
    public abstract Task<ArrayBuffer> DecodeBlock(ArrayBuffer buffer);
}