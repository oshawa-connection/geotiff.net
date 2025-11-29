using System;

namespace Geotiff.JavaScriptCompatibility;

public class ArrayBuffer
{
    private readonly byte[] buffer;

    public int Length => buffer.Length;

    public Span<byte> AsSpan()
    {
        return buffer.AsSpan();
    }

    public Memory<byte> AsMemory()
    {
        return buffer.AsMemory();
    }

    public static async Task<ArrayBuffer> FromStream(Stream stream, CancellationToken? signal)
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

        // stream.Position = 0;
        var ab = new ArrayBuffer(ms.ToArray());
        return ab;
    }

    public ArrayBuffer(byte[] backing)
    {
        buffer = backing;
    }

    public ArrayBuffer(int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        buffer = new byte[length];
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


    public byte[] GetBytes(int index, int nBytes)
    {
        // TODO: could use a span here?
        return buffer.Skip(index).Take(nBytes).ToArray();
    }

    public byte GetByte(int index)
    {
        if (index < 0 || index >= buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return buffer[index];
    }

    public void SetByte(int index, byte value)
    {
        if (index < 0 || index >= buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        buffer[index] = value;
    }

    public void Fill(byte value, int start = 0, int? end = null)
    {
        int actualEnd = end ?? buffer.Length;

        if (start < 0 || start > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (actualEnd < start || actualEnd > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(end));
        }

        for (int i = start; i < actualEnd; i++)
        {
            buffer[i] = value;
        }
    }
}