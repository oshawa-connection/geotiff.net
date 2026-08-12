using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

/// <summary>

/// </summary>
public static class DecoderRegistry
{
    private static List<GeoTiffDecoder> _register = new()
    {
        new DeflateGeoTiffDecoder(),
        new RawGeoTiffDecoder(),
        new LZWGeoTiffDecoder(),
        new PackBitsGeoTiffDecoder(),
        new JpegGeoTiffDecoder(),
        new ZSTDGeoTiffDecoder()
    };

    /// <summary>
    /// If user passes a code that already exists, replace it.
    /// </summary>
    /// <param name="geoTiffDecoder"></param>
    public static void AddDecoder(GeoTiffDecoder geoTiffDecoder)
    {
        if (geoTiffDecoder == null)
        {
            throw new ArgumentNullException(nameof(geoTiffDecoder));
        }

        var newCodes = geoTiffDecoder.codes.ToHashSet();

        // Remove any existing decoder that supports at least one of these codes
        _register.RemoveAll(existing =>
            existing.codes.Any(code => newCodes.Contains(code))
        );

        _register.Add(geoTiffDecoder);
    }

    public static GeoTiffDecoder GetDecoder(GeoTiffImage image)
    {
        var compressionTag = image.GetTag(TagFields.Compression);
        if (compressionTag is null)
        {
            return new RawGeoTiffDecoder();
        }
        
        int? compressionCode = compressionTag.GetUShort();
        

        var found = _register.FirstOrDefault(d => d.codes.Contains((int)compressionCode));
        if (found is null)
        {
            throw new GeoTiffException($"No decompression method registered for code: {compressionCode}");
        }

        return found;
    }

    public static async Task<byte[]> DecodeAsync(GeoTiffImage image, byte[] buffer, int predictor)
    {
        GeoTiffDecoder? decoder = GetDecoder(image);
        return await decoder.DecodeAsync(buffer, image, predictor);
    }

    public static byte[] Decode(GeoTiffImage image, byte[] buffer, int predictor)
    {
        GeoTiffDecoder? decoder = GetDecoder(image);
        return decoder.Decode(buffer, image, predictor);
    }
}