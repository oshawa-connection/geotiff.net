namespace Geotiff.Compression;

public abstract class Decoder
{
    public abstract IEnumerable<int> codes { get; set; }
    public abstract byte[] DecodeBlock();
}