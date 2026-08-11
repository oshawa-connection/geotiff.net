using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using ZstdSharp;

namespace Geotiff.Compression;

public class ZSTDGeoTiffDecoder : GeoTiffDecoder
{
    public override IEnumerable<int> codes => new[] { 50000,34887 };
    protected override async Task<byte[]> DecodeBlockAsync(byte[] buffer, GeoTiffImage image)
    {
        using var ms = new MemoryStream(buffer);
        using var outputFileStream = new MemoryStream();
        await using var ds = new DecompressionStream(ms);
        await ds.CopyToAsync(outputFileStream);
        
        var outArray = outputFileStream.ToArray();
        return outArray;
    }

    protected override byte[] DecodeBlock(byte[] buffer, GeoTiffImage image)
    {
        throw new NotImplementedException();
    }
}