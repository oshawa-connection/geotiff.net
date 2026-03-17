using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

/// <summary>
/// TODO: Allow users to override the default implementations.
/// </summary>
public class DecoderRegistry
{
    private static List<GeotiffDecoder> _register = new() { new DeflateGeotiffDecoder(), new RawGeotiffDecoder(), new LZWGeotiffDecoder(), new PackBitsGeotiffDecoder() };

    /// <summary>
    /// TODO: check no clash of codes
    /// </summary>
    /// <param name="geotiffDecoder"></param>
    public void AddDecoder(GeotiffDecoder geotiffDecoder)
    {
        _register.Add(geotiffDecoder);
    }

    public GeotiffDecoder GetDecoder(ImageFileDirectory fileDirectory)
    {
        int? compressionCode = fileDirectory.GetFileDirectoryValueIntOrNull("Compression");
        if (compressionCode is null)
        {
            return new RawGeotiffDecoder();
        }

        var found = _register.FirstOrDefault(d => d.codes.Contains((int)compressionCode));
        if (found is null)
        {
            throw new GeoTiffException($"No decompression method registered for code: {compressionCode}");
        }

        return found;
    }

    public async Task<ArrayBuffer> DecodeAsync(ImageFileDirectory fileDirectory, ArrayBuffer buffer, int tileWidth, int tileHeight, int predictor, int[] bitsPerSample, int planarConfiguration)
    {
        GeotiffDecoder? decoder = GetDecoder(fileDirectory);
        return await decoder.Decode(buffer, tileWidth, tileHeight, predictor, bitsPerSample, planarConfiguration);
    }
}