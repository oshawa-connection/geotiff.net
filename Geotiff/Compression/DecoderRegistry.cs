using Geotiff.JavaScriptCompatibility;

namespace Geotiff.Compression;

public class DecoderRegistry
{
    private List<Decoder> _register = new List<Decoder>();

    /// <summary>
    /// TODO: check no clash of codes
    /// </summary>
    /// <param name="decoder"></param>
    public void AddDecoder(Decoder decoder)
    {
        _register.Add(decoder);
    }

    public Decoder? GetDecoder(ImageFileDirectory fileDirectory)
    {
        var compressionCode = fileDirectory.GetFileDirectoryValue<int>("Compression");
        return _register.FirstOrDefault(d => d.codes.Contains(compressionCode));
    }

    // public Task<ArrayBuffer> Decode()
    // {
    //     
    // }
}