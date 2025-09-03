using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

/// <summary>
/// TODO: Allow users to override the default implementations.
/// </summary>
public class DecoderRegistry
{
    
    private static List<GeotiffDecoder> _register = new List<GeotiffDecoder>()
    {
        new DeflateGeotiffDecoder(),
        new RawGeotiffDecoder()
    };

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
        var compressionCode = fileDirectory.GetFileDirectoryValue<int?>("Compression");
        if (compressionCode is null)
        {
            return new RawGeotiffDecoder();
        }
        return _register.First(d => d.codes.Contains((int)compressionCode));
    }

    public async Task<ArrayBuffer> Decode(ImageFileDirectory fileDirectory, ArrayBuffer buffer)
    {
        var decoder = this.GetDecoder(fileDirectory);
        return await decoder.DecodeBlock(buffer);
    }
    
}