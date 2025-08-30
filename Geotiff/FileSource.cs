namespace Geotiff;

public class FileSource : BaseSource
{
    private readonly Stream stream;
    public FileSource(Stream stream)
    {
        this.stream = stream;
    }
    public override async Task<byte[]> FetchSlice(Slice slice)
    {
        var x = new byte[slice.length];
        
        stream.Seek(slice.offset, SeekOrigin.Begin);
        
        var nReadBytes = await stream.ReadAsync(x, 0, slice.length);
        if (nReadBytes < slice.length)
        {
            throw new Exception("Not enough bytes");
        }

        return x;
    }
    
    
    public override async Task<byte[]> FetchSlice(Slice slice, CancellationToken cancellationToken)
    {
        var x = new byte[slice.length];
        
        stream.Seek(slice.offset, SeekOrigin.Begin);
        
        var nReadBytes = await stream.ReadAsync(x, 0, slice.length, cancellationToken);
        if (nReadBytes < slice.length)
        {
            throw new Exception("Not enough bytes");
        }

        return x;
    }
}