using Geotiff.Exceptions;

namespace Geotiff;

/// <summary>
/// This class needs to be kept public to allow users to override methods when creating child classes. 
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
    public ushort? GetFileDirectoryValueUShortOrNull(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUShort();
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
    
    public ulong GetFileDirectoryValueULong(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetULong();
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
    public uint GetFileDirectoryValueUInt(string key)
    {
        if (TagDictionary.TryGetValue(key, out var v))
        {
            return v.GetUInt();
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
    
    public bool HasTag(string tagName)
    {
        return TagDictionary.ContainsKey(tagName);
    }

    public ushort[]? bitsPerSampleCached;

    public ushort[] BitsPerSample
    {
        get
        {
            if (bitsPerSampleCached is null)
            {
                var tag = GetTag(TagFields.BitsPerSample);
                var bitsPerSampleArray = tag.GetUShortArray();
                bitsPerSampleCached = bitsPerSampleArray;
            }

            return bitsPerSampleCached;
        }
    }
    private ushort[]? sampleFormatCached = null;
    public ushort[]? SampleFormat
    {
        get
        {
            if (sampleFormatCached is null)
            {
                var sampleFormatTag = GetTag(TagFields.SampleFormat);
                if (sampleFormatTag is not null)
                {
                    sampleFormatCached = sampleFormatTag.GetUShortArray();    
                }
            }

            return sampleFormatCached;
        }
    }

    private byte[]? jpegTablesCached = null;

    public byte[]? JpegTables
    {
        get
        {
            if (jpegTablesCached is null)
            {
                var tag = GetTag("JPEGTables");
                if (tag is null)
                {
                    return null;
                }
                
                jpegTablesCached = tag.GetByteArray();
            }

            return jpegTablesCached;
        }
    }

    public string? GDAL_NODATA => GetFileDirectoryValueString("GDAL_NODATA");
    
    
    /// <summary>
    /// Returns null if the tag is not found in the ImageFileDirectory.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Tag? GetTag(string name)
    {
        var found = TagDictionary.TryGetValue(name, out Tag? tag);
        return found ? tag : null;
    }
    
    
    /// <summary>
    /// Returns null if the tag is not found in the ImageFileDirectory.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Tag? GetTag(int id)
    {
        var found = RawFileDirectory.TryGetValue(id, out Tag? tag);
        return found ? tag : null;
    }
}