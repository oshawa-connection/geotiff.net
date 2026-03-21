using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

/// <summary>
/// Any kind of streamed source, e.g. file streams, memory streams.
/// </summary>
public class FileSource : BaseSource
{
    private readonly Stream stream;

    public FileSource(Stream stream)
    {
        this.stream = stream;
    }

    public override async Task<byte[]> FetchSliceAsync(Slice slice, CancellationToken? cancellationToken)
    {
        byte[]? x = new byte[slice.Length];
        
        stream.Seek(slice.Offset, SeekOrigin.Begin);

        int nReadBytes = 0;
        if (cancellationToken is null)
        {
            nReadBytes = await stream.ReadAsync(x, 0, slice.Length);
        }
        else
        {
            nReadBytes = await stream.ReadAsync(x, 0, slice.Length, (CancellationToken)cancellationToken);
        }
        
        return x;
    }

    public override byte[] FetchSlice(Slice slice)
    {
        byte[]? x = new byte[slice.Length];

        stream.Seek(slice.Offset, SeekOrigin.Begin);

        int nReadBytes = stream.Read(x, 0, slice.Length); // ok if nReadBytes is less than requested
        
        return x;
    }
}