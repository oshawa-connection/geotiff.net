using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

/// <summary>
/// This is a special case, has a code but also gets selected if code is null.
/// </summary>
public class RawGeotiffDecoder : GeotiffDecoder
{
    public override IEnumerable<int> codes => new[] { 1 };

    public override async Task<ArrayBuffer> DecodeBlock(ArrayBuffer buffer)
    {
        return buffer;
    }
}