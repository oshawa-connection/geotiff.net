using Geotiff.Exceptions;
using Rationals;
using System.Text;

namespace Geotiff;

/// <summary>
/// This represents a Tag value that has been read but has no association with its parent Tag Id or mapped name.
/// The reading of the value and the tag id itself is done as two steps.
/// This could be merged with the Tag class, however, be sure to merge the logic for tag id mapping and
/// value reading into the same method where both are available at the same time.
/// </summary>
internal class GeoTiffTagValueResult
{
    private byte[] _resultByte;
    private sbyte[]? _resultSByte;
    private short[]? _resultInt16;
    private Int64[]? _resultInt64;
    private double[]? _resultFloat64;
    private float[]? _resultFloat32;
    private ulong[]? _resultUInt64;
    private uint[]? _resultUInt32;
    private int[]? _resultInt32;
    private ushort[]? _resultUInt16;
    private Rational[]? _resultRational;
    private int[]? _resultSRational;

    public bool IsByte => _resultByte is not null;
    public bool IsSByte => _resultSByte is not null;
    public bool IsInt16 => _resultInt16 is not null;
    
    public bool IsInt64 => _resultInt64 is not null;
    
    public bool IsFloat64 => _resultFloat64 is not null;
    public bool IsFloat32 => _resultFloat32 is not null;
    public bool IsUInt16 => _resultUInt16 is not null;
    public bool IsUInt64 => _resultUInt64 != null;
    public bool IsUInt32 => _resultUInt32 != null;
    public bool IsInt32 => _resultInt32 != null; 
    public bool IsRational => _resultRational != null;
    public bool IsSRational => _resultSRational != null;
    
    public ulong[] GetUInt64Array() =>
        _resultUInt64 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("UInt64", this.DataType);
    
    public ulong GetUInt64()
    {
        if (_resultUInt64 is null)
        {
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("UInt64", this.DataType);
        }

        return _resultUInt64.Single();
    }

    public short[] GetInt16Array() =>
        _resultInt16 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Int16", this.DataType);

    public short GetInt16()
    {
        if (_resultInt16 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("short", this.DataType);
        return _resultInt16.Single();
    }

    public byte[] GetByteArray() =>
        _resultByte ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("byte", this.DataType);
    
    public sbyte[] GetSByteArray() =>
        _resultSByte ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("sbyte[]", this.DataType);

    public sbyte GetSByte()
    {
        if (_resultSByte is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("sbyte", this.DataType);
        return _resultSByte.Single();
    }

    public long[] GetInt64Array() =>
        _resultInt64 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("long[]", this.DataType);

    public long GetInt64()
    {
        if (_resultInt64 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("long", this.DataType);
        return _resultInt64.Single();
    }

    public double[] GetFloat64Array() =>
        _resultFloat64 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("double[]", this.DataType);

    public double GetFloat64()
    {
        if (_resultFloat64 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("double", this.DataType);
        return _resultFloat64.Single();
    }

    public float[] GetFloat32Array() =>
        _resultFloat32 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("float[]", this.DataType);

    public float GetFloat32()
    {
        if (_resultFloat32 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("float", this.DataType);
        return _resultFloat32.Single();
    }

    public ushort[] GetUInt16Array() =>
        _resultUInt16 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("ushort", this.DataType);

    public ushort GetUInt16()
    {
        if (_resultUInt16 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("ushort", this.DataType);
        return _resultUInt16.Single();
    }

    public uint[] GetUInt32Array() =>
        _resultUInt32 ??  throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("uint[]", this.DataType);

    public uint GetUInt32()
    {
        if (_resultUInt32 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("uint", this.DataType);
        return _resultUInt32.Single();
    }

    public int[] GetInt32Array() =>
        _resultInt32 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("int[]", this.DataType);

    public int GetInt32()
    {
        if (_resultInt32 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("int", this.DataType);
        return _resultInt32.Single();
    }

    public Rational[] GetRationalArray() =>
        _resultRational ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.RATIONAL.ToString(), this.DataType);

    public Rational GetRational()
    {
        if (_resultRational is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.RATIONAL.ToString(), this.DataType);
        return _resultRational.Single();
    }

    public int[] GetSRationalArray() =>
        _resultSRational ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.SRATIONAL.ToString(), this.DataType);

    public int GetSRational()
    {
        if (_resultSRational is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.SRATIONAL.ToString(), this.DataType);
        return _resultSRational.Single();
    }

    public string GetString()
    {
        if (this.IsByte)
        {
            return System.Text.Encoding.ASCII.GetString(_resultByte);

        }
        
        throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.BYTE.ToString(), this.DataType);
    }
        
    
    public static GeoTiffTagValueResult FromSBytes(sbyte[] data)
    {
        return new GeoTiffTagValueResult { _resultSByte = data };
    }
    
    public static GeoTiffTagValueResult FromUInt64(ulong[] data)
    {
        return new GeoTiffTagValueResult { _resultUInt64 = data };
    }

    public static GeoTiffTagValueResult FromInt64(Int64[] data)
    {
        return new GeoTiffTagValueResult { _resultInt64 = data };
    }

    public static GeoTiffTagValueResult FromUInt16(ushort[] data)
    {
        return new GeoTiffTagValueResult { _resultUInt16 = data };
    }
    
    public static GeoTiffTagValueResult FromInt16(short[] data)
    {
        return new GeoTiffTagValueResult { _resultInt16 = data };
    }
    
    public static GeoTiffTagValueResult FromUInt32(uint[] data)
    {
        return new GeoTiffTagValueResult { _resultUInt32 = data };
    }

    public static GeoTiffTagValueResult FromInt32(int[] data)
    {
        return new GeoTiffTagValueResult() { _resultInt32 = data };
    }

    public static GeoTiffTagValueResult FromFloat32(float[] data)
    {
        return new GeoTiffTagValueResult { _resultFloat32 = data };
    }

    public static GeoTiffTagValueResult FromFloat64(double[] data)
    {
        return new GeoTiffTagValueResult { _resultFloat64 = data };
    }

    public static GeoTiffTagValueResult FromRational(Rational[] data)
    {
        return new GeoTiffTagValueResult { _resultRational = data };
    }

    public static GeoTiffTagValueResult FromSRational(int[] data)
    {
        return new GeoTiffTagValueResult { _resultSRational = data };
    }

    public static GeoTiffTagValueResult FromByte(byte[] data)
    {
        return new GeoTiffTagValueResult() { _resultByte = data };
    }
    
    [Obsolete]
    private Array GetList()
    {
        if (IsFloat64 is true)
        {
            return _resultFloat64;
        }

        if (IsFloat32)
        {
            return _resultFloat32;
        }

        if (IsUInt64)
        {
            return _resultUInt64;
        }

        if (IsUInt32)
        {
            return _resultUInt32;
        }

        if (IsUInt16)
        {
            return _resultUInt16;
        }

        if (IsByte)
        {
            return _resultByte;
        }

        if (IsRational)
        {
            return _resultRational;
        }

        if (IsSByte)
        {
            return _resultSByte;
        }

        if (IsSRational)
        {
            return _resultSRational;
        }

        if (IsInt64)
        {
            return _resultInt64;
        }

        if (IsInt16)
        {
            return _resultInt16;
        }

        throw new InvalidOperationException("No result array is set.");
    }

    public object GetFirstElement()
    {
        return GetList().GetValue(0);
    }
    
    public List<object> GetArrayOfElements()
    {
        var objs = new List<object>();
        foreach (object? o in GetList())
        {
            objs.Add(o);
        }

        return objs;
    }
    
    /// <summary>
    /// Note that strings are byte arrays.
    /// </summary>
    /// <exception cref="GeoTiffException"></exception>
    private TagDataType DataType
    {
        get
        {
            if (this.IsInt16) return TagDataType.SHORT;
            if (this.IsSByte) return TagDataType.SBYTE;
            if (this.IsInt64) return TagDataType.SLONG8;
            if (this.IsByte) return TagDataType.BYTE; 
            if (this.IsFloat64) return TagDataType.DOUBLE;
            if (this.IsFloat32) return TagDataType.FLOAT;
            if (this.IsUInt16) return TagDataType.USHORT;
            if (this.IsUInt64) return TagDataType.ULONG;
            if (this.IsUInt32) return TagDataType.UINT;
            if (this.IsInt32) return TagDataType.INT;
            if (this.IsRational) return TagDataType.RATIONAL;
            if (this.IsSRational) return TagDataType.SRATIONAL;
            
            throw new GeoTiffException("Unrecognised tag type");
        }
    }
    
    
    private GeoTiffTagValueResult() { }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(this.DataType);
        var list = this.GetList();
        if (list.Length > 1)
        {
            sb.Append("[]");
            return sb.ToString();
        }

        sb.Append(" ");
        
        var firstElement = this.GetFirstElement();
        sb.Append(firstElement);

        return sb.ToString();
    }
}