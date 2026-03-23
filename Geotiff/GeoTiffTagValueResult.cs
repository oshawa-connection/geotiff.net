using Geotiff.Exceptions;
using Rationals;

namespace Geotiff;

/// <summary>
/// TODO: This class should be merged with the Tag class - this is a less user friendly version of that class that doesn't
/// store tag field name
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
    public bool IsUint16 => _resultUInt16 is not null;
    public bool IsUInt64 => _resultUInt64 != null;
    public bool IsUInt32 => _resultUInt32 != null;
    public bool IsInt32 => _resultInt32 != null; 
    public bool IsRational => _resultRational != null;
    public bool IsSRational => _resultSRational != null;
    
    public ulong[] GetUInt64Array() =>
        _resultUInt64 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("UInt64", this.DataType.ToString());
    
    public ulong GetUInt64()
    {
        if (_resultUInt64 is null)
        {
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("UInt64", this.DataType.ToString());
        }

        return _resultUInt64.First();
    }

    public short[] GetInt16Array() =>
        _resultInt16 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Int16", this.DataType.ToString());

    public short GetInt16()
    {
        if (_resultInt16 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Int16", this.DataType.ToString());
        return _resultInt16.First();
    }

    public byte[] GetByteArray() =>
        _resultByte ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Byte", this.DataType.ToString());
    
    public sbyte[] GetSByteArray() =>
        _resultSByte ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("SByte", this.DataType.ToString());

    public sbyte GetSByte()
    {
        if (_resultSByte is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("SByte", this.DataType.ToString());
        return _resultSByte.First();
    }

    public long[] GetInt64Array() =>
        _resultInt64 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Int64", this.DataType.ToString());

    public long GetInt64()
    {
        if (_resultInt64 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Int64", this.DataType.ToString());
        return _resultInt64.First();
    }

    public double[] GetFloat64Array() =>
        _resultFloat64 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Float64", this.DataType.ToString());

    public double GetFloat64()
    {
        if (_resultFloat64 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Float64", this.DataType.ToString());
        return _resultFloat64.First();
    }

    public float[] GetFloat32Array() =>
        _resultFloat32 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Float32", this.DataType.ToString());

    public float GetFloat32()
    {
        if (_resultFloat32 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Float32", this.DataType.ToString());
        return _resultFloat32.First();
    }

    public ushort[] GetUInt16Array() =>
        _resultUInt16 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("UInt16", this.DataType.ToString());

    public ushort GetUInt16()
    {
        if (_resultUInt16 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("UInt16", this.DataType.ToString());
        return _resultUInt16.First();
    }

    public uint[] GetUInt32Array() =>
        _resultUInt32 ??  throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("UInt32", this.DataType.ToString());

    public uint GetUInt32()
    {
        if (_resultUInt32 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("UInt32", this.DataType.ToString());
        return _resultUInt32.First();
    }

    public int[] GetInt32Array() =>
        _resultInt32 ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Int32", this.DataType.ToString());

    public int GetInt32()
    {
        if (_resultInt32 is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes("Int32", this.DataType.ToString());
        return _resultInt32.First();
    }

    public Rational[] GetRationalArray() =>
        _resultRational ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.RATIONAL.ToString(), this.DataType.ToString());

    public Rational GetRational()
    {
        if (_resultRational is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.RATIONAL.ToString(), this.DataType.ToString());
        return _resultRational.First();
    }

    public int[] GetSRationalArray() =>
        _resultSRational ?? throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.SRATIONAL.ToString(), this.DataType.ToString());

    public int GetSRational()
    {
        if (_resultSRational is null)
            throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.SRATIONAL.ToString(), this.DataType.ToString());
        return _resultSRational.First();
    }

    public string GetString()
    {
        if (this.IsByte)
        {
            return System.Text.Encoding.ASCII.GetString(_resultByte);

        }
        
        throw GeoTiffTagInvalidOperationException.FromExceptedActualTypes(TagDataType.BYTE.ToString(), this.DataType.ToString());
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

        if (IsUint16)
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
    public TagDataType DataType
    {
        get
        {
            if (this.IsInt16) return TagDataType.SSHORT;
            if (this.IsSByte) return TagDataType.SBYTE;
            if (this.IsInt64) return TagDataType.SLONG8;
            if (this.IsByte) return TagDataType.BYTE; 
            if (this.IsFloat64) return TagDataType.DOUBLE;
            if (this.IsFloat32) return TagDataType.FLOAT;
            if (this.IsUint16) return TagDataType.SHORT;
            if (this.IsUInt64) return TagDataType.LONG8;
            if (this.IsUInt32) return TagDataType.LONG;
            if (this.IsInt32) return TagDataType.SLONG;
            if (this.IsRational) return TagDataType.RATIONAL;
            if (this.IsSRational) return TagDataType.SRATIONAL;
            
            throw new GeoTiffException("Unrecognised tag type");
        }
    }
    
    
    private GeoTiffTagValueResult() { }

    public override string ToString()
    {
        return $"{this.}"
    }
}