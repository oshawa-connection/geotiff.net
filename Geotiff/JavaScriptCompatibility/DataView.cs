namespace Geotiff.JavaScriptCompatibility;

internal class DataView
{
    private readonly byte[] stream;
    public readonly string? type;
    public bool IsTyped => type != null;

    public DataView(byte[] stream, string? type = null)
    {
        this.type = type;
        this.stream = stream;
    }

    public DataView(int size, string? type = null) : this(new byte[size], type) { }

    public DataView(ArrayBuffer buffer)
    {
        stream = buffer.GetAllBytes(); // TODO: Watch memory usage here. Might create a copy?
    }

    public ArrayBuffer ToArrayBuffer()
    {
        return new ArrayBuffer(stream);
    }

    public const string UINT8 = "UINT8";
    public const string INT8 = "INT8";
    public const string UINT32 = "UINT32";
    public const string UINT64 = "UINT64";
    public const string INT32 = "INT32";
    public const string FLOAT = "FLOAT";
    public const string DOUBLE = "DOUBLE";

    private int GetFieldTypeLength(string type)
    {
        switch (type)
        {
            case UINT8:
            case INT8:
                return 1;
            case FLOAT:
                return 4;
            case DOUBLE:
                return 8;
            default:
                throw new NotImplementedException($"{type} not implemented");
        }
    }


    public int Length => stream.Length;

    private void SetByteRange(int offset, byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            stream[offset + i] = bytes[i];
        }
    }

    private void CheckType(string type, bool read)
    {
        if (this.type == null)
        {
            return;
        }

        if (this.type != type)
        {
            string? x = read ? "read" : "write";
            throw new Exception($"Invalid operation, trying to {x} a {type} on array of type {this.type}");
        }
    }

    public float GetFloat32(int offset, bool isLittleEndian = false)
    {
        CheckType(FLOAT, true);
        byte[]? x = stream.Skip(offset).Take(4).ToArray();
        if (x.Count() < 4)
        {
            throw new Exception("Not enough bytes in stream");
        }

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToSingle(x);
    }

    public void SetFloat32(int offset, float value, bool isLittleEndian = false)
    {
        CheckType(FLOAT, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    public double GetFloat64(int offset, bool isLittleEndian = false)
    {
        CheckType(DOUBLE, true);
        byte[]? x = stream.Skip(offset).Take(8).ToArray();
        if (x.Count() < 8)
        {
            throw new Exception("Not enough bytes in stream");
        }

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToDouble(x);
    }

    public void SetFloat64(int offset, double value, bool isLittleEndian = false)
    {
        CheckType(DOUBLE, false);
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
            throw new Exception("Not enough bytes in stream");
        }

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return x;
    }

    public short GetInt16(int offset, bool isLittleEndian = false)
    {
        byte[]? x = Read16(offset, isLittleEndian);

        return BitConverter.ToInt16(x);
    }


    public ushort GetUint16(int offset, bool isLittleEndian = false)
    {
        byte[]? x = Read16(offset, isLittleEndian);
        return BitConverter.ToUInt16(x);
    }

    public byte GetUint8(int offset)
    {
        CheckType(UINT8, true);
        return stream[offset];
    }

    public void SetUint8(int offset, byte value)
    {
        CheckType(UINT8, false);
        stream[offset] = value;
    }


    public sbyte GetInt8(int offset)
    {
        CheckType(INT8, true);
        return (sbyte)stream.Skip(offset).First();
    }

    public void Setint8(int offset, byte value)
    {
        CheckType(INT8, false);
        stream[offset] = value;
    }


    public uint GetUint32(int offset, bool isLittleEndian = false)
    {
        CheckType(UINT32, true);
        byte[]? x = stream.Skip(offset).Take(4).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToUInt32(x);
    }

    public void SetUint32(int offset, uint value, bool isLittleEndian = false)
    {
        CheckType(UINT32, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    public UInt64 GetUint64(int offset, bool isLittleEndian = false)
    {
        CheckType(UINT64, true);
        byte[]? x = stream.Skip(offset).Take(8).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToUInt64(x);
    }
    

    public int GetInt32(int offset, bool isLittleEndian = false)
    {
        CheckType(INT32, true);
        byte[]? x = stream.Skip(offset).Take(4).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToInt32(x);
    }

    public void SetInt32(int offset, int value, bool isLittleEndian = false)
    {
        CheckType(UINT32, false);
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

        switch (type)
        {
            case UINT8:
                SetUint8(offset, (byte)value);
                break;
            case INT8:
                Setint8(offset, (byte)value);
                break;
            case FLOAT:
                SetFloat32(offset, (float)value);
                break;
            case DOUBLE:
                SetFloat64(offset, (float)value);
                break;
        }
    }
}