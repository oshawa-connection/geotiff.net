using Geotiff.Exceptions;
using Rationals;

namespace Geotiff;

public class Tag
{
    public int RawId { get; }
    public string? TagName { get; }
    public bool IsArray { get; }

    public int Length =>  this.Value.Length;

    private GeoTiffTagValueResult Value { get; set; }
    public string GetString() {
        var s = Value.GetString();
        if (s.EndsWith("\0"))
        {
            s = s.Substring(0, s.Length - 1);
        }
        return s;
    }

    public override string ToString()
    {
        return $"{this.TagName}: {this.Value}";
    }
    
    /// <summary>
    /// Useful in the case where the type of object isn't important, e.g. Console.WriteLine
    /// If you call this on an Array Tag, it will return the first value of that array.
    /// </summary>
    /// <returns></returns>
    public object GetFirstObject() => this.Value.GetFirstElement();
    /// <summary>
    /// Useful in the case where the type of object isn't important, e.g. Console.WriteLine
    /// </summary>
    /// <returns></returns>
    public IEnumerable<object> GetArrayOfObjects() => this.Value.GetArrayOfElements();
    public ulong[] GetULongArray() => this.Value.GetUInt64Array();
    public ulong GetULong()
    {
        return this.Value.GetUInt64();
    }

    public short[] GetShortArray() => this.Value.GetInt16Array();
    public short GetShort() => this.Value.GetInt16();
    public sbyte[] GetSByteArray()=> this.Value.GetSByteArray();
    public sbyte GetSByte()=> this.Value.GetSByte();
    public long[] GetLongArray()=> this.Value.GetInt64Array();

    public byte[] GetByteArray()=> this.Value.GetByteArray();
    public byte GetByte()=> this.Value.GetByte();
    
    public long GetLong()
    {
        return this.Value.GetInt64();
    }

    public double[] GetDoubleArray() => Value.GetFloat64Array();

    public double GetDouble()
    {
        return this.Value.GetFloat64();  
    } 
    public float[] GetFloatArray()=> this.Value.GetFloat32Array();
    public float GetFloat()=> this.Value.GetFloat32();
    public ushort[] GetUShortArray()=> this.Value.GetUInt16Array();
    public ushort GetUShort() => this.Value.GetUInt16();
    public uint[] GetUIntArray()=> this.Value.GetUInt32Array();

    public uint GetUInt()
    {
        return this.Value.GetUInt32();    
    }
    public int[] GetIntArray()=> this.Value.GetInt32Array();

    public int GetInt()
    {
        return this.Value.GetInt32();    
    }
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
        if (Value.IsDouble)
            return Value.GetFloat64();
        if (Value.IsFloat)
            return (double)Value.GetFloat32();
        if (Value.IsLong)
            return (double)Value.GetInt64();
        if (Value.IsULong)
            return (double)Value.GetUInt64();
        if (Value.IsInt)
            return (double)Value.GetInt32();
        if (Value.IsUInt)
            return (double)Value.GetUInt32();
        if (Value.IsShort)
            return (double)Value.GetInt16();
        if (Value.IsUShort)
            return (double)Value.GetUInt16();
        if (Value.IsSByte)
            return (double)Value.GetSByte();
        if (Value.IsRational)
            return (double)Value.GetRational();
        if (Value.IsSRational)
            return (double)Value.GetSRational();

        throw new GeoTiffException("Tag does not contain a numeric value.");
    }
    
    /// <summary>
    /// Converts all elements to double so long as value is a numeric type
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    public double[] GetAsDoubleArray()
    {
        if (Value.IsDouble)
            return Value.GetFloat64Array();
        if (Value.IsFloat)
            return Value.GetFloat32Array().Select(f => (double)f).ToArray();
        if (Value.IsLong)
            return Value.GetInt64Array().Select(l => (double)l).ToArray();
        if (Value.IsULong)
            return Value.GetUInt64Array().Select(ul => (double)ul).ToArray();
        if (Value.IsInt)
            return Value.GetInt32Array().Select(i => (double)i).ToArray();
        if (Value.IsUInt)
            return Value.GetUInt32Array().Select(ui => (double)ui).ToArray();
        if (Value.IsShort)
            return Value.GetInt16Array().Select(s => (double)s).ToArray();
        if (Value.IsUShort)
            return Value.GetUInt16Array().Select(us => (double)us).ToArray();
        if (Value.IsByte)
            return Value.GetByteArray().Select(sb => (double)sb).ToArray();
        if (Value.IsSByte)
            return Value.GetSByteArray().Select(sb => (double)sb).ToArray();
        if (Value.IsRational)
            return Value.GetRationalArray().Select(r => (double)r).ToArray();
        if (Value.IsSRational)
            return Value.GetSRationalArray().Select(sr => (double)sr).ToArray();
        
        throw new GeoTiffException("Tag does not contain a numeric array value.");
    }
    
    /// <summary>
    /// Converts element to int so long as value is a numeric type
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    public int GetAsInt()
    {
        if (Value.IsDouble)
            return (int)Value.GetFloat64();
        if (Value.IsFloat)
            return (int)Value.GetFloat32();
        if (Value.IsLong)
            return (int)Value.GetInt64();
        if (Value.IsULong)
            return (int)Value.GetUInt64();
        if (Value.IsInt)
            return (int)Value.GetInt32();
        if (Value.IsUInt)
            return (int)Value.GetUInt32();
        if (Value.IsShort)
            return (int)Value.GetInt16();
        if (Value.IsUShort)
            return (int)Value.GetUInt16();
        if (Value.IsSByte)
            return (int)Value.GetSByte();
        if (Value.IsRational)
            return (int)Value.GetRational();
        if (Value.IsSRational)
            return (int)Value.GetSRational();

        throw new GeoTiffException("Tag does not contain a numeric value.");
    }
    
    /// <summary>
    /// Converts all elements to int so long as value is a numeric type
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    public int[] GetAsIntArray()
    {
        if (Value.IsDouble)
            return Value.GetFloat64Array().Select(f => (int)f).ToArray();
        if (Value.IsFloat)
            return Value.GetFloat32Array().Select(f => (int)f).ToArray();
        if (Value.IsLong)
            return Value.GetInt64Array().Select(l => (int)l).ToArray();
        if (Value.IsULong)
            return Value.GetUInt64Array().Select(ul => (int)ul).ToArray();
        if (Value.IsInt)
            return Value.GetInt32Array().Select(i => (int)i).ToArray();
        if (Value.IsUInt)
            return Value.GetUInt32Array().Select(ui => (int)ui).ToArray();
        if (Value.IsShort)
            return Value.GetInt16Array().Select(s => (int)s).ToArray();
        if (Value.IsUShort)
            return Value.GetUInt16Array().Select(us => (int)us).ToArray();
        if (Value.IsByte)
            return Value.GetByteArray().Select(sb => (int)sb).ToArray();
        if (Value.IsSByte)
            return Value.GetSByteArray().Select(sb => (int)sb).ToArray();
        if (Value.IsRational)
            return Value.GetRationalArray().Select(r => (int)r).ToArray();
        if (Value.IsSRational)
            return Value.GetSRationalArray().Select(sr => (int)sr).ToArray();
        
        throw new GeoTiffException("Tag does not contain a numeric array value.");
    }
    
    /// <summary>
    /// Some tags are stored as either ushort/ ulong.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    public ulong GetAsULong()
    {
        if (Value.IsDouble)
            return (ulong)Value.GetFloat64();
        if (Value.IsFloat)
            return (ulong)Value.GetFloat32();
        if (Value.IsLong)
            return (ulong)Value.GetInt64();
        if (Value.IsULong)
            return (ulong)Value.GetUInt64();
        if (Value.IsInt)
            return (ulong)Value.GetInt32();
        if (Value.IsUInt)
            return (ulong)Value.GetUInt32();
        if (Value.IsShort)
            return (ulong)Value.GetInt16();
        if (Value.IsUShort)
            return (ulong)Value.GetUInt16();
        if (Value.IsSByte)
            return (ulong)Value.GetSByte();
        if (Value.IsRational)
            return (ulong)Value.GetRational();
        if (Value.IsSRational)
            return (ulong)Value.GetSRational();

        throw new GeoTiffException("Tag does not contain a numeric value.");
    }
    
    /// <summary>
    /// Converts all elements to long so long as value is a numeric type
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    public ulong[] GetAsULongArray()
    {
        if (Value.IsDouble)
            return Value.GetFloat64Array().Select(f => (ulong)f).ToArray();
        if (Value.IsFloat)
            return Value.GetFloat32Array().Select(f => (ulong)f).ToArray();
        if (Value.IsLong)
            return Value.GetInt64Array().Select(l => (ulong)l).ToArray();
        if (Value.IsULong)
            return Value.GetUInt64Array().Select(ul => (ulong)ul).ToArray();
        if (Value.IsInt)
            return Value.GetInt32Array().Select(i => (ulong)i).ToArray();
        if (Value.IsUInt)
            return Value.GetUInt32Array().Select(ui => (ulong)ui).ToArray();
        if (Value.IsShort)
            return Value.GetInt16Array().Select(s => (ulong)s).ToArray();
        if (Value.IsUShort)
            return Value.GetUInt16Array().Select(us => (ulong)us).ToArray();
        if (Value.IsByte)
            return Value.GetByteArray().Select(sb => (ulong)sb).ToArray();
        if (Value.IsSByte)
            return Value.GetSByteArray().Select(sb => (ulong)sb).ToArray();
        if (Value.IsRational)
            return Value.GetRationalArray().Select(r => (ulong)r).ToArray();
        if (Value.IsSRational)
            return Value.GetSRationalArray().Select(sr => (ulong)sr).ToArray();
        
        throw new GeoTiffException("Tag does not contain a numeric array value.");
    }


    public TagDataType DataType
    {
        get
        {
            if (this.IsArray)
            {
                if (this.Value.IsShort) return TagDataType.SHORT_ARRAY;
                if (this.Value.IsSByte) return TagDataType.SBYTE_ARRAY;
                if (this.Value.IsLong) return TagDataType.LONG_ARRAY;
                if (this.Value.IsByte) return TagDataType.BYTE_ARRAY;
                if (this.Value.IsDouble) return TagDataType.DOUBLE_ARRAY;
                if (this.Value.IsFloat) return TagDataType.FLOAT_ARRAY;
                if (this.Value.IsUShort) return TagDataType.SHORT_ARRAY;
                if (this.Value.IsULong) return TagDataType.ULONG_ARRAY;
                if (this.Value.IsUInt) return TagDataType.UINT_ARRAY;
                if (this.Value.IsInt) return TagDataType.INT_ARRAY;
                if (this.Value.IsRational) return TagDataType.RATIONAL_ARRAY;
                if (this.Value.IsSRational) return TagDataType.SRATIONAL_ARRAY;
            }
            else
            {
                if (this.Value.IsString) return TagDataType.ASCII;
                if (this.Value.IsShort) return TagDataType.SHORT;
                if (this.Value.IsSByte) return TagDataType.SBYTE;
                if (this.Value.IsLong) return TagDataType.LONG;
                if (this.Value.IsByte) return TagDataType.BYTE;
                if (this.Value.IsDouble) return TagDataType.DOUBLE;
                if (this.Value.IsFloat) return TagDataType.FLOAT;
                if (this.Value.IsUShort) return TagDataType.USHORT;
                if (this.Value.IsULong) return TagDataType.ULONG;
                if (this.Value.IsUInt) return TagDataType.UINT;
                if (this.Value.IsInt) return TagDataType.INT;
                if (this.Value.IsRational) return TagDataType.RATIONAL;
                if (this.Value.IsSRational) return TagDataType.SRATIONAL;
            }

            throw new GeoTiffException("Unrecognised tag type");
        }
    }

    

    internal Tag(int rawId, string? tagName, GeoTiffTagValueResult value, bool isArray)
    {
        this.RawId = rawId;
        this.TagName = tagName;
        this.Value = value;
        this.IsArray = isArray;
    }
}