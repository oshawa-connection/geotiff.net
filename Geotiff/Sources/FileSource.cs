using Geotiff.Extensions;
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

    public override async Task<ArrayBuffer> FetchSliceAsync(Slice slice, CancellationToken? cancellationToken)
    {
        byte[]? x = new byte[slice.Length];
        stream.Seek(slice.Offset, SeekOrigin.Begin);
        await stream.ReadWholeArrayAsync(x, cancellationToken);
        return new ArrayBuffer(x);
    }

    public override ArrayBuffer FetchSlice(Slice slice)
    {
        byte[]? x = new byte[slice.Length];

        stream.Seek(slice.Offset, SeekOrigin.Begin);

        int nReadBytes = stream.Read(x, 0, slice.Length);
        
        return new ArrayBuffer(x);
    }
}