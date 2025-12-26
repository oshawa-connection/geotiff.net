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
        IEnumerable<T>? finalResult = null;
        bool listReadResult = TagDictionary.TryGetValue(key, out var listOfObjects);
        if (listReadResult is true)
        {
            finalResult = ((List<object>)listOfObjects.Value).UnboxAll<T>();
        }

        return finalResult;
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
            object? converted = Convert.ChangeType(obj, targetType);
            return (T)converted;
        }

        return default!;
    }


    public T GetFileDirectoryValue<T>(string key)
    {
        if (TagDictionary.TryGetValue(key, out Tag? obj))
        {
            Type? targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            object? converted = Convert.ChangeType(obj.Value, targetType);
            return (T)converted;
        }

        return default!;
    }
    
    public int[] BitsPerSample => GetFileDirectoryArrayValue<int>("BitsPerSample");

    public int[]? SampleFormat => GetFileDirectoryArrayValue<int>("SampleFormat");

    public string? GDAL_NODATA => GetFileDirectoryValue<string>("GDAL_NODATA");
}