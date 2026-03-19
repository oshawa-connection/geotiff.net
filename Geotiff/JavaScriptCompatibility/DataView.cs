using Geotiff.Exceptions;

namespace Geotiff.JavaScriptCompatibility;

internal class DataView
{
    private readonly byte[] stream;
    public readonly GeotiffSampleDataType? type;
    public bool IsTyped => type != null;

    public DataView(byte[] stream, GeotiffSampleDataType? type = null)
    {
        this.type = type;
        this.stream = stream;
    }

    public DataView(int size, GeotiffSampleDataType? type = null) : this(new byte[size], type) { }

    /// <summary>
    /// TODO: Check if the ctor is necessary, and if so, if a datatype can be defined.
    /// </summary>
    /// <param name="buffer"></param>
    public DataView(ArrayBuffer buffer)
    {
        stream = buffer.GetAllBytes(); // TODO: Watch memory usage here. Might create a copy?
    }
    

    public ArrayBuffer ToArrayBuffer()
    {
        return new ArrayBuffer(stream);
    }
    
    public int Length => stream.Length;

    public DataView Copy()
    {
        return new DataView((byte[])this.stream.Clone(), this.type);
    }
    
    private void SetByteRange(int offset, byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            stream[offset + i] = bytes[i];
        }
    }

    private void CheckType(GeotiffSampleDataType type, bool read)
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
    /// <summary>
    /// Because we are targeting .netstandard2.1, Half datatype is not supported. Read 2 bytes then convert to float32
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="isLittleEndian"></param>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    public float GetFloat16(int offset, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.Float16, true);

        byte[]? x = stream.Skip(offset).Take(2).ToArray();
        if (x.Length < 2)
        {
            throw new GeoTiffException("Not enough bytes in stream");
        }
        
        if (!isLittleEndian)
        {
            x = x.Reverse().ToArray();
        }

        ushort half = BitConverter.ToUInt16(x, 0);

        return HalfToSingle(half);
    }

    private static float HalfToSingle(ushort half)
    {
        uint sign = (uint)(half >> 15) & 0x00000001;
        uint exp  = (uint)(half >> 10) & 0x0000001F;
        uint mant = (uint)(half & 0x03FF);

        uint f;

        if (exp == 0)
        {
            if (mant == 0)
            {
                // Zero
                f = sign << 31;
            }
            else
            {
                // Subnormal → normalize
                while ((mant & 0x0400) == 0)
                {
                    mant <<= 1;
                    exp--;
                }
                exp++;
                mant &= ~0x0400U;

                exp = exp + (127 - 15);
                mant <<= 13;

                f = (sign << 31) | (exp << 23) | mant;
            }
        }
        else if (exp == 31)
        {
            // Inf or NaN
            f = (sign << 31) | 0x7F800000 | (mant << 13);
        }
        else
        {
            // Normalized number
            exp = exp + (127 - 15);
            mant <<= 13;

            f = (sign << 31) | (exp << 23) | mant;
        }

        return BitConverter.Int32BitsToSingle((int)f);
    }
    
    public float GetFloat32(int offset, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.Float32, true);
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

    public float GetFloat32ElementOffset(int elementOffset, bool isLittleEndian = false)
    {
        return GetFloat32(elementOffset * 4, isLittleEndian);
    }

    public void SetFloat32(int byteOffset, float value, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.Float32, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(byteOffset, x);
    }


    public void SetFloat32ElementOffset(int elementOffset, float value, bool isLittleEndian = false)
    {
        this.SetFloat32(elementOffset * 4, value, isLittleEndian);
    }

    public double GetFloat64(int offset, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.Float64, true);
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
        CheckType(GeotiffSampleDataType.Float64, false);
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
        CheckType(GeotiffSampleDataType.Int16, true);
        byte[]? x = Read16(offset, isLittleEndian);

        return BitConverter.ToInt16(x);
    }
    

    public ushort GetUint16(int offset, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.UInt16, true);
        byte[]? x = Read16(offset, isLittleEndian);
        return BitConverter.ToUInt16(x);
    }

    public byte GetUint8(int offset)
    {
        CheckType(GeotiffSampleDataType.UInt8, true);
        return stream[offset];
    }

    public void SetUint8(int offset, byte value)
    {
        CheckType(GeotiffSampleDataType.UInt8, false);
        stream[offset] = value;
    }


    public sbyte GetInt8(int offset)
    {
        CheckType(GeotiffSampleDataType.Int8, true);
        return (sbyte)stream.Skip(offset).First();
    }

    public void Setint8(int offset, byte value)
    {
        CheckType(GeotiffSampleDataType.Int8, false);
        stream[offset] = value;
    }


    public uint GetUint32(int offset, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.UInt32, true);
        byte[]? x = stream.Skip(offset).Take(4).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToUInt32(x);
    }

    public void SetUint32(int offset, uint value, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.UInt32, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    public UInt64 GetUint64(int offset, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.UInt64, true);
        byte[]? x = stream.Skip(offset).Take(8).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToUInt64(x);
    }
    

    public int GetInt32(int offset, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.Int32, true);
        byte[]? x = stream.Skip(offset).Take(4).ToArray();

        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        return BitConverter.ToInt32(x);
    }

    public void SetInt32(int offset, int value, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.UInt32, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    public void SetInt16(int offset, short value, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.Int16, false);
        byte[]? x = BitConverter.GetBytes(value);
        if (isLittleEndian is false)
        {
            x = x.Reverse().ToArray();
        }

        SetByteRange(offset, x);
    }

    [Obsolete("Use typed versions of this method")]
    public void SetValue(int offset, object value)
    {
        if (IsTyped is false)
        {
            throw new InvalidOperationException($"Cannot setValue on an typed array");
        }

        // TODO: add support for other data types
        switch (type)
        {
            case GeotiffSampleDataType.UInt8:
                SetUint8(offset, (byte)value);
                break;
            case GeotiffSampleDataType.Int8:
                Setint8(offset, (byte)value);
                break;
            case GeotiffSampleDataType.Float32:
                SetFloat32(offset, (float)value);
                break;
            case GeotiffSampleDataType.Float64:
                SetFloat64(offset, (float)value);
                break;
        }
    }
}