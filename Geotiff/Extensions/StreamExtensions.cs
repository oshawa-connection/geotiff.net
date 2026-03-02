namespace Geotiff.Extensions;

public static class StreamExtensions
{
    /// <summary>
    /// Having less bytes than requested is ok. Up to specified amount, but less is ok.
    /// Adapted from https://jonskeet.uk/csharp/readbinary.html
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    public static async Task ReadWholeArrayAsync(this Stream stream, byte[] data,CancellationToken? cancellationToken = null)
    {
        int offset=0;
        int remaining = data.Length;
        
        while (remaining > 0)
        {
            int read = 0;

            if (cancellationToken is null)
            {
                read = await stream.ReadAsync(data, offset, remaining);    
            }
            else
            {
                read = await stream.ReadAsync(data, offset, remaining, (CancellationToken)cancellationToken);
            }
            
            if (read <= 0)
            {
                return;
            }
            remaining -= read;
            offset += read;
        }
    }
    
    /// <summary>
    /// Having less bytes than requested is ok. Up to specified amount, but less is ok.
    /// Adapted from https://jonskeet.uk/csharp/readbinary.html
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="data"></param>
    public static void ReadWholeArray(this Stream stream, byte[] data)
    {
        int offset=0;
        int remaining = data.Length;
        while (remaining > 0)
        {
            int read = stream.Read(data, offset, remaining);
            if (read <= 0)
            {
                return;
            }
            remaining -= read;
            offset += read;
        }
    }
}