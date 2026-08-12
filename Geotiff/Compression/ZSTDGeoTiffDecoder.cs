using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using ZstdSharp;

namespace Geotiff.Compression;

public class ZSTDGeoTiffDecoder : GeoTiffDecoder
{
    public override IEnumerable<int> codes => new[] { 50000 };
    protected override async Task<byte[]> DecodeBlockAsync(byte[] buffer, GeoTiffImage image)
    {
        using var ms = new MemoryStream(buffer);
        await using var ds = new DecompressionStream(ms);
        using var outputFileStream = new MemoryStream();
        await ds.CopyToAsync(outputFileStream);
        return outputFileStream.ToArray();
    }

    protected override byte[] DecodeBlock(byte[] buffer, GeoTiffImage image)
    {
        using var ms = new MemoryStream(buffer);
        using var ds = new DecompressionStream(ms);
        using var outputFileStream = new MemoryStream();
        ds.CopyTo(outputFileStream);
        return outputFileStream.ToArray();
    }
}