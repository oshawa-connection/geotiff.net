using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

/// <summary>
/// TODO: Allow users to override the default implementations.
/// </summary>
public class DecoderRegistry
{
    private static List<GeoTiffDecoder> _register = new()
    {
        new DeflateGeoTiffDecoder(), 
        new RawGeoTiffDecoder(), 
        new LZWGeoTiffDecoder(), 
        new PackBitsGeoTiffDecoder(),
        new JpegGeoTiffDecoder()
    };

    /// <summary>
    /// If user passes a code that already exists, replace it.
    /// </summary>
    /// <param name="geoTiffDecoder"></param>
    public void AddDecoder(GeoTiffDecoder geoTiffDecoder)
    {
        _register.Add(geoTiffDecoder);
    }

    public GeoTiffDecoder GetDecoder(ImageFileDirectory fileDirectory)
    {
        var compressionTag = fileDirectory.GetTag(TagFields.Compression);
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

    public async Task<byte[]> DecodeAsync(ImageFileDirectory fileDirectory, GeoTiffImage image, byte[] buffer, int predictor)
    {
        GeoTiffDecoder? decoder = GetDecoder(fileDirectory);
        return await decoder.Decode(buffer, image, predictor);
    }
}