using System.Collections;

namespace Geotiff.JavaScriptCompatibility;

public class ArrayBuffer : IEnumerable<byte>
{
    private readonly byte[] buffer;

    public int Length => buffer.Length;
    
    public Span<byte> SpanSlice(int start, int length)
    {
        Span<byte> bytes = buffer;
        return bytes.Slice(start: start, length: length);
    }
    
    public static async Task<ArrayBuffer> FromStreamAsync(Stream stream, CancellationToken? signal= null)
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
        
        var ab = new ArrayBuffer(ms.ToArray());
        return ab;
    }

    public ArrayBuffer(byte[] backing)
    {
        buffer = backing;
    }
    
    public ArrayBuffer(long length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        buffer = new byte[length];
    }

    public byte[] GetAllBytes()
    {
        return buffer;
    }
    
    public IEnumerator<byte> GetEnumerator()
    {
        return this.buffer.Cast<byte>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}