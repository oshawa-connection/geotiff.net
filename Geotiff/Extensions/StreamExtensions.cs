namespace Geotiff.Extensions;

public static class StreamExtensions
{
    public static async Task<byte[]> ToByteArray(this Stream stream, CancellationToken? signal= null)
    {
        var ms = new MemoryStream();
        if (signal != null)
        {
            await stream.CopyToAsync(ms, (CancellationToken)signal);
        }
        else
        {
            await stream.CopyToAsync(ms);
        }
        
        return ms.ToArray();
    }
}