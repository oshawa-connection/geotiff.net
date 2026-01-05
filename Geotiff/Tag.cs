using Geotiff.Exceptions;
using Geotiff.Primitives;

namespace Geotiff;

public class Tag
{
    public int RawId { get; }
    public string? TagName { get; }
    private GeotiffTagValueResult Value { get; set; }
    public string GetString() => this.Value.GetString();
    public ulong[] GetULongArray() => this.Value.GetUInt64Array();
    public ulong GetULong() => this.Value.GetUInt64();
    public short[] GetShortArray() => this.Value.GetInt16Array();
    public short GetShort()=> this.Value.GetInt16();
    public sbyte[] GetSByteArray()=> this.Value.GetSByteArray();
    public sbyte GetSByte()=> this.Value.GetSByte();
    public long[] GetLongArray()=> this.Value.GetInt64Array();
    public long GetLong()=> this.Value.GetInt64();
    public double[] GetDoubleArray() => this.Value.GetFloat64Array();
    public double GetDouble()=> this.Value.GetFloat64();
    public float[] GetFloatArray()=> this.Value.GetFloat32Array();
    public float GetFloat()=> this.Value.GetFloat32();
    public ushort[] GetUShortArray()=> this.Value.GetUInt16Array();
    public ushort GetUShort()=> this.Value.GetUInt16();
    public uint[] GetUIntArray()=> this.Value.GetUInt32Array();
    public uint GetUInt()=> this.Value.GetUInt32();
    public int[] GetIntArray()=> this.Value.GetInt32Array();
    public int GetInt()=> this.Value.GetInt32();
    public Rational[] GetRationalArray()=> this.Value.GetRationalArray();
    public Rational GetRational()=> this.Value.GetRational();
    public int[] GetSRationalArray() => this.Value.GetSRationalArray();
    public int GetSRational() => this.Value.GetSRational();

    /// <summary>
    /// Converts to double so long as value is a numeric type
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    public double GetAsDouble()
    {
        if (Value.IsFloat64)
            return Value.GetFloat64();
        if (Value.IsFloat32)
            return (double)Value.GetFloat32();
        if (Value.IsInt64)
            return (double)Value.GetInt64();
        if (Value.IsUInt64)
            return (double)Value.GetUInt64();
        if (Value.IsInt32)
            return (double)Value.GetInt32();
        if (Value.IsUInt32)
            return (double)Value.GetUInt32();
        if (Value.IsInt16)
            return (double)Value.GetInt16();
        if (Value.IsUint16)
            return (double)Value.GetUInt16();
        if (Value.IsSByte)
            return (double)Value.GetSByte();
        if (Value.IsRational)
            return Value.GetRational().ToDouble();
        if (Value.IsSRational)
            return (double)Value.GetSRational();

        throw new GeoTiffException("Tag does not contain a numeric value.");
    }
    
    
    public double[] GetAsDoubleArray()
    {
        if (Value.IsFloat64)
            return Value.GetFloat64Array();
        if (Value.IsFloat32)
            return Value.GetFloat32Array().Select(f => (double)f).ToArray();
        if (Value.IsInt64)
            return Value.GetInt64Array().Select(l => (double)l).ToArray();
        if (Value.IsUInt64)
            return Value.GetUInt64Array().Select(ul => (double)ul).ToArray();
        if (Value.IsInt32)
            return Value.GetInt32Array().Select(i => (double)i).ToArray();
        if (Value.IsUInt32)
            return Value.GetUInt32Array().Select(ui => (double)ui).ToArray();
        if (Value.IsInt16)
            return Value.GetInt16Array().Select(s => (double)s).ToArray();
        if (Value.IsUint16)
            return Value.GetUInt16Array().Select(us => (double)us).ToArray();
        if (Value.IsSByte)
            return Value.GetSByteArray().Select(sb => (double)sb).ToArray();
        if (Value.IsRational)
            return Value.GetRationalArray().Select(r => r.ToDouble()).ToArray();
        if (Value.IsSRational)
            return Value.GetSRationalArray().Select(sr => (double)sr).ToArray();

        throw new GeoTiffException("Tag does not contain a numeric array value.");
    }


    public TagDataType DataType
    {
        get
        {
            if (this.IsArray)
            {
                if (this.Value.IsInt16) return TagDataType.SHORT_ARRAY;
                if (this.Value.IsSByte) return TagDataType.SBYTE_ARRAY;
                if (this.Value.IsInt64) return TagDataType.LONG8_ARRAY;
                if (this.Value.IsString) return TagDataType.ASCII; // Strings are not arrays, but for completeness
                if (this.Value.IsFloat64) return TagDataType.DOUBLE_ARRAY;
                if (this.Value.IsFloat32) return TagDataType.FLOAT_ARRAY;
                if (this.Value.IsUint16) return TagDataType.SHORT_ARRAY;
                if (this.Value.IsUInt64) return TagDataType.LONG8_ARRAY;
                if (this.Value.IsUInt32) return TagDataType.LONG_ARRAY;
                if (this.Value.IsInt32) return TagDataType.SLONG_ARRAY;
                if (this.Value.IsRational) return TagDataType.RATIONAL_ARRAY;
                if (this.Value.IsSRational) return TagDataType.SRATIONAL_ARRAY;
            }
            else
            {
                if (this.Value.IsInt16) return TagDataType.SSHORT;
                if (this.Value.IsSByte) return TagDataType.SBYTE;
                if (this.Value.IsInt64) return TagDataType.SLONG8;
                if (this.Value.IsString) return TagDataType.ASCII;
                if (this.Value.IsFloat64) return TagDataType.DOUBLE;
                if (this.Value.IsFloat32) return TagDataType.FLOAT;
                if (this.Value.IsUint16) return TagDataType.SHORT;
                if (this.Value.IsUInt64) return TagDataType.LONG8;
                if (this.Value.IsUInt32) return TagDataType.LONG;
                if (this.Value.IsInt32) return TagDataType.SLONG;
                if (this.Value.IsRational) return TagDataType.RATIONAL;
                if (this.Value.IsSRational) return TagDataType.SRATIONAL;
            }

            throw new GeoTiffException("Unrecognised tag type");
        }
    }

    public bool IsArray { get; }

    internal Tag(int rawId, string? tagName, GeotiffTagValueResult value, bool isArray)
    {
        this.RawId = rawId;
        this.TagName = tagName;
        this.Value = value;
        this.IsArray = isArray;
    }
}