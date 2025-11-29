namespace Geotiff;

using System.Collections.Generic;

public class ImageFileDirectory
{
    /// <summary>
    /// Mapping of tag names to values.
    /// </summary>
    public Dictionary<string, object> FileDirectory { get; }

    /// <summary>
    /// Mapping of tag IDs to values (raw representation).
    /// </summary>
    public Dictionary<int, object> RawFileDirectory { get; }

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
    /// <param name="fileDirectory">Mapping tag names to values.</param>
    /// <param name="rawFileDirectory">Raw file directory, mapping tag IDs to values.</param>
    /// <param name="geoKeyDirectory">Geo key directory, mapping geo key names to values.</param>
    /// <param name="nextIFDByteOffset">Byte offset to the next IFD.</param>
    public ImageFileDirectory(
        Dictionary<string, object> fileDirectory,
        Dictionary<int, object> rawFileDirectory,
        Dictionary<string, object> geoKeyDirectory,
        int nextIFDByteOffset)
    {
        FileDirectory = fileDirectory;
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
        bool listReadResult = FileDirectory.TryGetValue(key, out object listOfObjects);
        if (listReadResult is true)
        {
            finalResult = ((List<object>)listOfObjects).UnboxAll<T>();
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
        if (FileDirectory.TryGetValue(key, out object obj))
        {
            Type? targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            object? converted = Convert.ChangeType(obj, targetType);
            return (T)converted;
        }

        return default!;
    }

    public int[] BitsPerSample => GetFileDirectoryArrayValue<int>("BitsPerSample");

    public int[]? SampleFormat => GetFileDirectoryArrayValue<int>("SampleFormat");

    public string? GDAL_NODATA => GetFileDirectoryValue<string>("GDAL_NODATA");
}