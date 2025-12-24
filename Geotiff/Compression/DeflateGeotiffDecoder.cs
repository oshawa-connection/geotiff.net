using Geotiff.JavaScriptCompatibility;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors;

namespace Geotiff.Compression;

public class DeflateGeotiffDecoder : GeotiffDecoder
{
    public override IEnumerable<int> codes => new[] { 8, 32946 };

    /// <summary>
    /// DOn't know how efficient this is. Difficult to manage because we might not read the bytes of the tiff in one go!
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public override async Task<ArrayBuffer> DecodeBlockAsync(ArrayBuffer buffer)
    {
        using var ms = new MemoryStream(buffer.GetAllBytes());
        await using var decompressor = new ZlibStream(ms, CompressionMode.Decompress);
        var outputFileStream = new MemoryStream();
        await decompressor.CopyToAsync(outputFileStream);
        return new ArrayBuffer(outputFileStream.ToArray());
    }
}