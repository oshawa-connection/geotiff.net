using System;
using System.Buffers.Binary;
using Geotiff.Exceptions;

namespace Geotiff.JavaScriptCompatibility;

internal class DataView
{
    private readonly byte[] _buffer;
    public readonly GeotiffSampleDataType? Type;

    public DataView(byte[] buffer, GeotiffSampleDataType? type = null)
    {
        _buffer = buffer;
        Type = type;
    }

    public DataView(int size, GeotiffSampleDataType? type = null)
        : this(new byte[size], type)
    {
    }

    public Span<byte> Span => _buffer;
    public ReadOnlySpan<byte> ReadOnlySpan => _buffer;

    public int Length => _buffer.Length;

    private void CheckType(GeotiffSampleDataType expected, bool read)
    {
        if (Type is null)
            return;

        if (Type != expected)
        {
            string op = read ? "read" : "write";
            throw new GeoTiffException(
                $"Invalid operation, trying to {op} a {expected} on an array of type {Type}"
            );
        }
    }

    private static bool IsLittleEndian(bool? isLittleEndian)
        => isLittleEndian ?? BitConverter.IsLittleEndian;

    private static Span<byte> Slice(byte[] buffer, int offset, int size)
        => buffer.AsSpan(offset, size);

    // -----------------------
    // Int16
    // -----------------------
    public short GetInt16(int offset, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Int16, true);

        var span = ReadOnlySpan.Slice(offset, 2);

        return IsLittleEndian(littleEndian)
            ? BinaryPrimitives.ReadInt16LittleEndian(span)
            : BinaryPrimitives.ReadInt16BigEndian(span);
    }

    public void SetInt16(int offset, short value, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Int16, false);

        var span = Span.Slice(offset, 2);

        if (IsLittleEndian(littleEndian))
            BinaryPrimitives.WriteInt16LittleEndian(span, value);
        else
            BinaryPrimitives.WriteInt16BigEndian(span, value);
    }

    // -----------------------
    // UInt16
    // -----------------------
    public ushort GetUInt16(int offset, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.UInt16, true);

        var span = ReadOnlySpan.Slice(offset, 2);

        return IsLittleEndian(littleEndian)
            ? BinaryPrimitives.ReadUInt16LittleEndian(span)
            : BinaryPrimitives.ReadUInt16BigEndian(span);
    }

    // -----------------------
    // Int32
    // -----------------------
    public int GetInt32(int offset, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Int32, true);

        var span = ReadOnlySpan.Slice(offset, 4);

        return IsLittleEndian(littleEndian)
            ? BinaryPrimitives.ReadInt32LittleEndian(span)
            : BinaryPrimitives.ReadInt32BigEndian(span);
    }

    public void SetInt32(int offset, int value, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Int32, false);

        var span = Span.Slice(offset, 4);

        if (IsLittleEndian(littleEndian))
            BinaryPrimitives.WriteInt32LittleEndian(span, value);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, value);
    }

    // -----------------------
    // UInt32
    // -----------------------
    public uint GetUInt32(int offset, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.UInt32, true);

        var span = ReadOnlySpan.Slice(offset, 4);

        return IsLittleEndian(littleEndian)
            ? BinaryPrimitives.ReadUInt32LittleEndian(span)
            : BinaryPrimitives.ReadUInt32BigEndian(span);
    }

    public void SetUInt32(int offset, uint value, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.UInt32, false);

        var span = Span.Slice(offset, 4);

        if (IsLittleEndian(littleEndian))
            BinaryPrimitives.WriteUInt32LittleEndian(span, value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(span, value);
    }

    // -----------------------
    // Int64
    // -----------------------
    public long GetInt64(int offset, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Int64, true);

        var span = ReadOnlySpan.Slice(offset, 8);

        return IsLittleEndian(littleEndian)
            ? BinaryPrimitives.ReadInt64LittleEndian(span)
            : BinaryPrimitives.ReadInt64BigEndian(span);
    }

    public void SetInt64(int offset, long value, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Int64, false);

        var span = Span.Slice(offset, 8);

        if (IsLittleEndian(littleEndian))
            BinaryPrimitives.WriteInt64LittleEndian(span, value);
        else
            BinaryPrimitives.WriteInt64BigEndian(span, value);
    }

    // -----------------------
    // UInt64
    // -----------------------
    public ulong GetUInt64(int offset, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.UInt64, true);

        var span = ReadOnlySpan.Slice(offset, 8);

        return IsLittleEndian(littleEndian)
            ? BinaryPrimitives.ReadUInt64LittleEndian(span)
            : BinaryPrimitives.ReadUInt64BigEndian(span);
    }

    
    public double GetFloat64(int offset, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Float64, true);

        var span = ReadOnlySpan.Slice(offset, 8);

        long bits = IsLittleEndian(littleEndian)
            ? BinaryPrimitives.ReadInt64LittleEndian(span)
            : BinaryPrimitives.ReadInt64BigEndian(span);

        return BitConverter.Int64BitsToDouble(bits);
    }

    public void SetFloat64(int offset, float value, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Float64, false);

        var span = Span.Slice(offset, 8);

        int bits = BitConverter.SingleToInt32Bits(value);

        if (IsLittleEndian(littleEndian))
            BinaryPrimitives.WriteInt32LittleEndian(span, bits);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, bits);
    }
    

    public float GetFloat32(int offset, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Float32, true);

        var span = ReadOnlySpan.Slice(offset, 4);

        int bits = IsLittleEndian(littleEndian)
            ? BinaryPrimitives.ReadInt32LittleEndian(span)
            : BinaryPrimitives.ReadInt32BigEndian(span);

        return BitConverter.Int32BitsToSingle(bits);
    }

    public void SetFloat32(int offset, float value, bool? littleEndian = null)
    {
        CheckType(GeotiffSampleDataType.Float32, false);

        var span = Span.Slice(offset, 4);

        int bits = BitConverter.SingleToInt32Bits(value);

        if (IsLittleEndian(littleEndian))
            BinaryPrimitives.WriteInt32LittleEndian(span, bits);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, bits);
    }


    public float GetFloat16(int offset, bool isLittleEndian = false)
    {
        CheckType(GeotiffSampleDataType.Float16, true);

        byte[]? x = _buffer.Skip(offset).Take(2).ToArray();
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
                // Subnormal -> normalize
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


    // -----------------------
    // Byte / Int8
    // -----------------------
    public byte GetUInt8(int offset)
    {
        CheckType(GeotiffSampleDataType.UInt8, true);
        return _buffer[offset];
    }

    public void SetUInt8(int offset, byte value)
    {
        CheckType(GeotiffSampleDataType.UInt8, false);
        _buffer[offset] = value;
    }

    public sbyte GetInt8(int offset)
    {
        CheckType(GeotiffSampleDataType.Int8, true);
        return unchecked((sbyte)_buffer[offset]);
    }

    public void SetInt8(int offset, sbyte value)
    {
        CheckType(GeotiffSampleDataType.Int8, false);
        _buffer[offset] = unchecked((byte)value);
    }
}