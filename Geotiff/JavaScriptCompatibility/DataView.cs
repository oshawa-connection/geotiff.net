namespace Geotiff.JavaScriptCompatibility;

public class DataView
{
    private readonly byte[] stream;
    public readonly string? type;
    public bool IsTyped => this.type != null;
    public DataView(byte[] stream, string? type = null)
    {
        this.type = type;
        this.stream = stream;
    }

    public DataView(int size, string? type = null): this(new byte[size], type) { }

    public ArrayBuffer ToArrayBuffer()
    {
        return new ArrayBuffer(stream);
    }
    
    public const string UINT8 = "UINT8";
    public const string INT8 = "INT8";
    public const string UINT32 = "UINT32";
    public const string INT32 = "INT32";
    public const string FLOAT = "FLOAT";
    public const string DOUBLE = "DOUBLE";
    
    private int GetFieldTypeLength(string type)
    {
        switch (type)
        {
            case UINT8: case INT8:
                return 1;
            case FLOAT:
                return 4;
            case DOUBLE:
                return 8;
            default:
                throw new NotImplementedException($"{type} not implemented");
        }
    }
    
    public DataView(ArrayBuffer buffer)
    {
        this.stream = buffer.GetAllBytes(); // TODO: Watch memory usage here. Might create a copy?
    }
    
    public int length
    {
        get
        {
            return this.stream.Length;
        }
    }

    private void setByteRange(int offset, byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            stream[offset + i] = bytes[i];
        }
    }

    private void checkType(string type, bool read)
    {
        if (this.type == null)
        {
            return;
        }

        if (this.type != type)
        {
            var x = read ? "read" : "write";
            throw new Exception($"Invalid operation, trying to {x} a {type} on array of type {this.type}");
        }
    }
    
    public float getFloat32(int offset, bool isLittleEndian = false)
    {
        checkType(FLOAT, true);
        var x= stream.Skip(offset).Take(4).ToArray();
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

    public void setFloat32(int offset, float value, bool isLittleEndian = false)
    {
        checkType(FLOAT, false);
        var x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        setByteRange(offset, x);
    }
    
    public double getFloat64(int offset, bool isLittleEndian = false)
    {
        checkType(DOUBLE, true);
        var x= stream.Skip(offset).Take(8).ToArray();
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
    
    public void setFloat64(int offset, double value, bool isLittleEndian = false)
    {
        checkType(DOUBLE, false);
        var x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        setByteRange(offset, x);
    }

    private byte[] read16(int offset, bool isLittleEndian = false)
    {
        var x= stream.Skip(offset).Take(2).ToArray();
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
    
    public Int16 getInt16(int offset)
    {
        var x = read16(offset);

        return BitConverter.ToInt16(x);
    }
    

    
    public UInt16 getUint16(int offset, bool isLittleEndian = false)
    {
        var x = read16(offset,isLittleEndian);
        return BitConverter.ToUInt16(x);
    }

    public byte getUint8(int offset)
    {
        checkType(UINT8, true);
        return stream[offset];
    }
    
    public void setUint8(int offset, byte value)
    {
        checkType(UINT8, false);
        stream[offset] = value;
    }
    
    
    public byte getInt8(int offset)
    {
        checkType(INT8, true);
        return stream.Skip(offset).First();
    }
    
    public void setint8(int offset, byte value)
    {
        checkType(INT8, false);
        stream[offset] = value;
    }


    public uint getUint32(int offset, bool isLittleEndian = false)
    {
        checkType(UINT32, true);
        var x = stream.Skip(offset).Take(4).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToUInt32(x);
    }
    
    public void setUint32(int offset, uint value, bool isLittleEndian = false)
    {
        checkType(UINT32, false);
        var x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        setByteRange(offset, x);
    }
    
    
    public int getInt32(int offset, bool isLittleEndian = false)
    {
        checkType(INT32, true);
        var x = stream.Skip(offset).Take(4).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToInt32(x);
    }
    
    public void setInt32(int offset, int value, bool isLittleEndian = false)
    {
        checkType(UINT32, false);
        var x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        setByteRange(offset, x);
    }

    public void SetValue(int offset, object value)
    {
        if (this.IsTyped is false)
        {
            throw new InvalidOperationException($"Cannot setValue on an typed array");
        }

        switch (this.type)
        {
            case UINT8:
                this.setUint8(offset, (byte) value);
                break;
            case INT8:
                this.setint8(offset, (byte) value);
                break;
            case FLOAT:
                this.setFloat32(offset, (float)value);
                break;
            case DOUBLE:
                this.setFloat64(offset, (float)value);
                break;
        }
    }
}