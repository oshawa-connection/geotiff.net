using Geotiff.Exceptions;
using Rationals;

namespace Geotiff;

using System.Collections.Generic;

/// <summary>

/// </summary>
public class ImageFileDirectory
{
    /// <summary>
    /// Mapping of tag names to values.
    /// </summary>
    public Dictionary<string, Tag> TagDictionary { get; }

    /// <summary>
    /// Mapping of tag IDs to values (raw representation).
    /// Could be useful if there are non-standard and non-GDAL tags added.
    /// </summary>
    public Dictionary<int, Tag> RawFileDirectory { get; }

    /// <summary>
    /// Mapping of geo key names to values.
    /// </summary>
    public Dictionary<string, object> GeoKeyDirectory { get; }

    /// <summary>
    /// Byte offset to the next IFD (Image File Directory).
    /// </summary>
    public int NextIFDByteOffset { get; }

    /// <summary>
    /// Creates an ImageFileDirectory.
    /// </summary>
    /// <param name="tagDictionary">Mapping tag names to values.</param>
    /// <param name="rawFileDirectory">Raw file directory, mapping tag IDs to values.</param>
    /// <param name="geoKeyDirectory">Geo key directory, mapping geo key names to values.</param>
    /// <param name="nextIFDByteOffset">Byte offset to the next IFD.</param>
    public ImageFileDirectory(
        Dictionary<string, Tag> tagDictionary,
        Dictionary<int, Tag> rawFileDirectory,
        Dictionary<string, object> geoKeyDirectory,
        int nextIFDByteOffset)
    {
        TagDictionary = tagDictionary;
        RawFileDirectory = rawFileDirectory;
        GeoKeyDirectory = geoKeyDirectory;
        NextIFDByteOffset = nextIFDByteOffset;
    }

    /// <summary>
    /// 
    /// TODO: Replace with GetFileDirectoryArrayValue to maintain JS compatibility as much as possible
    /// </summary>
    ///
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T>? GetFileDirectoryListValue<T>(string key)
    {
        if (TagDictionary.TryGetValue(key, out var tag))
        {
            if (typeof(T) == typeof(string))
            {
                var str = tag.GetString();
                return new[] { (T)(object)str };
            }
            else
            {
                //TODO: Do this the long form way where we check for every numeric type to prevent double conversion
                var arr = tag.GetAsDoubleArray();
                Type? targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                return arr.Select(d => (T)Convert.ChangeType(d, targetType));
            }
        }
        return null;
    }

    public T[]? GetFileDirectoryArrayValue<T>(string key)
    {
        IEnumerable<T>? found = GetFileDirectoryListValue<T>(key);

        if (found is null)
        {
            return null;
        }

        return found.ToArray();
    }

    public T GetGeoDirectoryValue<T>(string key)
    {
        if (GeoKeyDirectory.TryGetValue(key, out object obj))
        {
            Type? targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (obj == null)
            {
                return default!; 
            }
            object? converted = Convert.ChangeType(obj, targetType);
            return (T)converted;
        }

        return default!;
    }

    
    public ulong[]? GetFileDirectoryValueULongArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetULongArray();
        }
        return null;
    }
    public ulong? GetFileDirectoryValueULongOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetULong();
        }
        return null;
    }
    public short[]? GetFileDirectoryValueShortArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetShortArray();
        }
        return null;
    }
    public short? GetFileDirectoryValueShortOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetShort();
        }
        return null;
    }
    public sbyte[]? GetFileDirectoryValueSByteArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetSByteArray();
        }
        return null;
    }
    public sbyte? GetFileDirectoryValueSByteOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetSByte();
        }
        return null;
    }
    public long[]? GetFileDirectoryValueLongArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetLongArray();
        }
        return null;
    }
    public long? GetFileDirectoryValueLongOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetLong();
        }
        return null;
    }
    public double[]? GetFileDirectoryValueDoubleArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetDoubleArray();
        }
        return null;
    }
    public double? GetFileDirectoryValueDoubleOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetDouble();
        }
        return null;
    }
    public float[]? GetFileDirectoryValueFloatArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetFloatArray();
        }
        return null;
    }
    public float? GetFileDirectoryValueFloatOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetFloat();
        }
        return null;
    }
    public ushort[]? GetFileDirectoryValueUShortArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUShortArray();
        }
        return null;
    }
    public ushort? GetFileDirectoryValueUShortOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUShort();
        }
        return null;
    }
    public uint[]? GetFileDirectoryValueUIntArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUIntArray();
        }
        return null;
    }
    public uint? GetFileDirectoryValueUIntOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUInt();
        }
        return null;
    }
    public int[]? GetFileDirectoryValueIntArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetIntArray();
        }
        return null;
    }
    public int? GetFileDirectoryValueIntOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetInt();
        }
        return null;
    }
    public Rational[]? GetFileDirectoryValueRationalArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetRationalArray();
        }
        return null;
    }
    public Rational? GetFileDirectoryValueRationalOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetRational();
        }
        return null;
    }
    public int[]? GetFileDirectoryValueSRationalArrayOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetSRationalArray();
        }
        return null;
    }
    public int? GetFileDirectoryValueSRationalOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetSRational();
        }
        return null;
    }
    
        public ulong[] GetFileDirectoryValueULongArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetULongArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public ulong GetFileDirectoryValueULong(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetULong();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public short[] GetFileDirectoryValueShortArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetShortArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public short GetFileDirectoryValueShort(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetShort();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public sbyte[] GetFileDirectoryValueSByteArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetSByteArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public sbyte GetFileDirectoryValueSByte(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetSByte();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public long[] GetFileDirectoryValueLongArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetLongArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public long GetFileDirectoryValueLong(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetLong();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public double[] GetFileDirectoryValueDoubleArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetDoubleArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public double GetFileDirectoryValueDouble(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetDouble();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public float[] GetFileDirectoryValueFloatArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetFloatArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public float GetFileDirectoryValueFloat(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetFloat();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public ushort[] GetFileDirectoryValueUShortArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUShortArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public ushort GetFileDirectoryValueUShort(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUShort();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public uint[] GetFileDirectoryValueUIntArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUIntArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public uint GetFileDirectoryValueUInt(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUInt();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public int[] GetFileDirectoryValueIntArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetIntArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public int GetFileDirectoryValueInt(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetInt();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public Rational[] GetFileDirectoryValueRationalArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetRationalArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public Rational GetFileDirectoryValueRational(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetRational();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public int[] GetFileDirectoryValueSRationalArray(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetSRationalArray();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    public int GetFileDirectoryValueSRational(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetSRational();
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    
    public string? GetFileDirectoryValueString(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetString(); 
        }

        return null;
    }

    /// <summary>
    /// This promotes values to doubles GDAL style, and can be used when you don't mind loss of precision.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public double? GetFileDirectoryValueAsDoubleOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetAsDouble(); 
        }

        return null;
    }

    public double GetFileDirectoryValueAsDouble(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetAsDouble(); 
        }
        throw new GeoTiffTagKeyNotFoundException($"Tag '{key}' was not found.");
    }
    
    public Rational? GetFileDirectoryValueAsRational(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetRational();    
        }

        return null;
    }
    
    public bool HasTag(string tagName)
    {
        return this.TagDictionary.ContainsKey(tagName);
    }
    
    public int[] BitsPerSample => GetFileDirectoryArrayValue<int>("BitsPerSample");

    public int[]? SampleFormat => GetFileDirectoryArrayValue<int>("SampleFormat");

    public string? GDAL_NODATA => GetFileDirectoryValueString("GDAL_NODATA");
}