using Geotiff.JavaScriptCompatibility;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors;


// using System.IO;
// using System.IO.Compression;
namespace Geotiff.Compression;

public class DeflateGeoTiffDecoder : GeoTiffDecoder
{
    public override IEnumerable<int> codes => new[] { 8, 32946 };
    public static string BytesToHex(byte[] data)
    {
        return string.Join(" ", data.Select(b => b.ToString("x2")));
    }
    
    /// <summary>
    /// DOn't know how efficient this is. Difficult to manage because we might not read the bytes of the tiff in one go!
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    protected override async Task<ArrayBuffer> DecodeBlockAsync(ArrayBuffer buffer, GeoTiffImage image)
    {
        using var ms = new MemoryStream(buffer.GetAllBytes());
        
        using var outputFileStream = new MemoryStream();
        await using (var decompressor = new ZlibStream(ms, CompressionMode.Decompress))
        {
            await decompressor.CopyToAsync(outputFileStream);
        }

        var outArray = outputFileStream.ToArray();
        
        return new ArrayBuffer(outArray);
    }
}