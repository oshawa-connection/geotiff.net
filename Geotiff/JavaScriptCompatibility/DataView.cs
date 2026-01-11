using Geotiff.Exceptions;

namespace Geotiff.JavaScriptCompatibility;

internal class DataView
{
    private readonly byte[] stream;
    public readonly GeoTiffSampleDataType? type;
    public bool IsTyped => type != null;

    public DataView(byte[] stream, GeoTiffSampleDataType? type = null)
    {
        this.type = type;
        this.stream = stream;
    }

    public DataView(int size, GeoTiffSampleDataType? type = null) : this(new byte[size], type) { }

    public DataView(ArrayBuffer buffer)
    {
        stream = buffer.GetAllBytes(); // TODO: Watch memory usage here. Might create a copy?
    }

    public ArrayBuffer ToArrayBuffer()
    {
        return new ArrayBuffer(stream);
    }
    
    public int Length => stream.Length;

    private void SetByteRange(int offset, byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            stream[offset + i] = bytes[i];
        }
    }

    private void CheckType(GeoTiffSampleDataType type, bool read)
    {
        if (this.type == null)
        {
            return;
        }

        if (this.type != type)
        {
            string? op = read ? "read" : "write";
            throw new GeoTiffException($"Invalid operation, trying to {op} a {type} on an array of type {this.type}");
        }
    }

    public float GetFloat32(int offset, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.Float32, true);
        byte[]? x = stream.Skip(offset).Take(4).ToArray();
        if (x.Count() < 4)
        {
            throw new GeoTiffException("Not enough bytes in stream");
        }

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToSingle(x);
    }

    public void SetFloat32(int offset, float value, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.Float32, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    public double GetFloat64(int offset, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.Double, true);
        byte[]? x = stream.Skip(offset).Take(8).ToArray();
        if (x.Count() < 8)
        {
            throw new GeoTiffException("Not enough bytes in stream");
        }

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToDouble(x);
    }

    public void SetFloat64(int offset, double value, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.Double, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    private byte[] Read16(int offset, bool isLittleEndian = false)
    {
        byte[]? x = stream.Skip(offset).Take(2).ToArray();
        if (x.Count() < 2)
        {
            throw new GeoTiffException("Not enough bytes in stream");
        }

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return x;
    }

    public short GetInt16(int offset, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.Int16, true);
        byte[]? x = Read16(offset, isLittleEndian);

        return BitConverter.ToInt16(x);
    }


    public ushort GetUint16(int offset, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.UInt16, true);
        byte[]? x = Read16(offset, isLittleEndian);
        return BitConverter.ToUInt16(x);
    }

    public byte GetUint8(int offset)
    {
        CheckType(GeoTiffSampleDataType.Uint8, true);
        return stream[offset];
    }

    public void SetUint8(int offset, byte value)
    {
        CheckType(GeoTiffSampleDataType.Uint8, false);
        stream[offset] = value;
    }


    public sbyte GetInt8(int offset)
    {
        CheckType(GeoTiffSampleDataType.Int8, true);
        return (sbyte)stream.Skip(offset).First();
    }

    public void Setint8(int offset, byte value)
    {
        CheckType(GeoTiffSampleDataType.Int8, false);
        stream[offset] = value;
    }


    public uint GetUint32(int offset, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.UInt32, true);
        byte[]? x = stream.Skip(offset).Take(4).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToUInt32(x);
    }

    public void SetUint32(int offset, uint value, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.UInt32, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    public UInt64 GetUint64(int offset, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.UInt64, true);
        byte[]? x = stream.Skip(offset).Take(8).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToUInt64(x);
    }
    

    public int GetInt32(int offset, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.Int32, true);
        byte[]? x = stream.Skip(offset).Take(4).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToInt32(x);
    }

    public void SetInt32(int offset, int value, bool isLittleEndian = false)
    {
        CheckType(GeoTiffSampleDataType.UInt32, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    public void SetValue(int offset, object value)
    {
        if (IsTyped is false)
        {
            throw new InvalidOperationException($"Cannot setValue on an typed array");
        }

        // TODO: add support for other data types
        switch (type)
        {
            case GeoTiffSampleDataType.Uint8:
                SetUint8(offset, (byte)value);
                break;
            case GeoTiffSampleDataType.Int8:
                Setint8(offset, (byte)value);
                break;
            case GeoTiffSampleDataType.Float32:
                SetFloat32(offset, (float)value);
                break;
            case GeoTiffSampleDataType.Double:
                SetFloat64(offset, (float)value);
                break;
        }
    }
}