using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;
using Geotiff.RemoteClients;

namespace Geotiff;

/// <summary>
/// TODO: implement ReadRaster method here, selecting best image for the user. A nice to have.
/// </summary>
public class GeoTIFF
{
    protected internal readonly BaseSource Source;
    private readonly bool _bigTiff;
    protected internal readonly ulong FirstIFDOffset;
    public readonly bool IsLittleEndian;
    /// <summary>
    /// Prevents us making read requests if GetImageCount is called multiple times
    /// </summary>
    protected internal int? finalImageCount = null;
    public bool IsBifTIFF => _bigTiff; 
    
    public GeoTIFF(BaseSource source, bool isLittleEndian, bool bigTiff, ulong firstIFDOffset)
    {
        this.Source = source;
        this.IsLittleEndian = isLittleEndian;
        this._bigTiff = bigTiff;
        this.FirstIFDOffset = firstIFDOffset;
        this.finalImageCount = null;
    }
    
    private static bool GetBomMarker(DataView dv)
    {
        ushort value = dv.GetUint16(0, true);
        bool isLittleEndian = false;
        if (value == Constant.BOMLittleEndian)
        {
            isLittleEndian = true;
        }
        else if (value == Constant.BOMBigEndian)
        {
            isLittleEndian = false;
        }
        else
        {
            throw new InvalidTiffException("Unrecognised Tiff BOM marker");
        }

        return isLittleEndian;
    }

    private static bool GetBigTiffMarker(DataView dv, bool isLittleEndian)
    {
        ushort isBigTiffValue = dv.GetUint16(2, isLittleEndian);
        if (isBigTiffValue == 42)
        {
            return false;
        }

        if (isBigTiffValue == 43)
        {
            return true;
        }
        
        var offsetByteSize = dv.GetUint16(4, isLittleEndian);
        if (offsetByteSize != 8) {
            throw new InvalidTiffException("Unsupported offset byte-size.");
        }

        throw new InvalidTiffException("Invalid tiff magic number.");
    }
    
    private static ulong GetFirstIFDOffset(DataView dv, bool isLittleEndian, bool isBigTiff)
    {
        // This is used to 
        return isBigTiff
            ? dv.GetUint64(8, isLittleEndian)
            : dv.GetUint32(4, isLittleEndian);
    }
    
    public static async Task<GeoTIFF> FromRemoteClientAsync(IGeotiffRemoteClient client)
    {
        var source = new RemoteSource(client, int.MaxValue, false);
        IEnumerable<ArrayBuffer>? slices = await source.FetchAsync(new Slice[] { new(0, 1024) });

        var dv = new DataView(slices.First());
        bool isLittleEndian = GetBomMarker(dv);

        bool isBigTiff = GetBigTiffMarker(dv, isLittleEndian);

        var firstIDFOffset= GetFirstIFDOffset(dv, isLittleEndian, isBigTiff);
        return new GeoTIFF(source, isLittleEndian, isBigTiff, firstIDFOffset);
    }
    
    
    /// <summary>
    /// If you provide a non-seekable stream, the entire stream will be read into memory.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static GeoTIFF FromStream(Stream stream)
    {
        Stream seekableStream;
        if (stream.CanSeek)
        {
            seekableStream = stream;
        }
        else
        {
            seekableStream = new MemoryStream();
            stream.CopyTo(seekableStream);
            seekableStream.Position = 0;
        }
        
        byte[] buffer = new byte[1024];
        // having less bytes than requested is ok in this situation. Up to 1024, but less is ok.
        seekableStream.Read(buffer, 0, buffer.Length);
        
        byte[]? arr = buffer.ToArray();
        var dv = new DataView(arr);
        bool isLittleEndian = GetBomMarker(dv);

        bool isBigTiff = GetBigTiffMarker(dv, isLittleEndian);
        
        var firstIDFOffset= GetFirstIFDOffset(dv, isLittleEndian, isBigTiff);
        seekableStream.Position = 0;
        var source = new FileSource(seekableStream);
        return new GeoTIFF(source, isLittleEndian, isBigTiff, firstIDFOffset);
    }
    
    
    /// <summary>
    /// If you provide a non-seekable stream, the entire stream will be read into memory.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<GeoTIFF> FromStreamAsync(Stream stream, CancellationToken? cancellationToken = null)
    {
        Stream seekableStream;
        if (stream.CanSeek)
        {
            seekableStream = stream;
        }
        else
        {
            seekableStream = new MemoryStream();
            if (cancellationToken is not null)
            {
                await stream.CopyToAsync(seekableStream,(CancellationToken)cancellationToken);    
            }
            else
            {
                await stream.CopyToAsync(seekableStream);
            }
            
            seekableStream.Position = 0;
        }
        
        byte[] buffer = new byte[1024];
        // having less bytes than requested is ok in this situation. Up to 1024, but less is ok.
        if (cancellationToken is not null)
        {
            await seekableStream.ReadAsync(buffer, 0, buffer.Length, (CancellationToken)cancellationToken);    
        }
        else
        {
            await seekableStream.ReadAsync(buffer, 0, buffer.Length);
        }
         
        
        byte[]? arr = buffer.ToArray();
        var dv = new DataView(arr);
        ushort value = dv.GetUint16(0, true);
        bool isLittleEndian = GetBomMarker(dv);

        bool isBigTiff = GetBigTiffMarker(dv, isLittleEndian);
        
        var firstIDFOffset= GetFirstIFDOffset(dv, isLittleEndian, isBigTiff);
        seekableStream.Position = 0;
        var source = new FileSource(seekableStream);
        return new GeoTIFF(source, isLittleEndian, isBigTiff, firstIDFOffset);
    }

    private async Task<DataSlice> GetSliceAsync(int offset, int? size = null)
    {
        int fallbackSize = _bigTiff ? 4048 : 1024;
        int sizeToUse = size is null ? fallbackSize : (int)size;
        var slice = new Slice(offset, sizeToUse, false);
        var slices = new List<Slice>() { slice };
        IEnumerable<ArrayBuffer>? results = await Source.FetchAsync(slices);

        return new DataSlice(
            results.Single().GetAllBytes(), // TODO: Double check this.  
            offset,
            IsLittleEndian,
            _bigTiff
        );
    }


    private Dictionary<string, object>? ParseGeoKeyDirectory(Dictionary<string, Tag> fileDirectory)
    {
        bool rawGeoKeyDirectoryResult = fileDirectory.TryGetValue("GeoKeyDirectory", out Tag rawGeoKeyDirectoryObj);
        
        if (!rawGeoKeyDirectoryResult)
        {
            return null;
        }

        ushort[]? rawGeoKeyDirectory = ((List<object>)rawGeoKeyDirectoryObj.Value).UnboxAll<ushort>().ToArray();

        Dictionary<string, object> geoKeyDirectory = new();
        for (int i = 4; i <= rawGeoKeyDirectory[3] * 4; i += 4)
        {
            string key = FieldTypes.GeoKeyNames.GetByKey(rawGeoKeyDirectory[i]);
            // string key = FieldTypes.GeoKeyNames[rawGeoKeyDirectory[i]];
            // TODO: try find a tif where this is 0. Not clear what value it should be, perhaps undefined in JS means array isn't set?
            string? location = rawGeoKeyDirectory[i + 1] != 0
                ? FieldTypes.FieldTags.GetByKey(rawGeoKeyDirectory[i + 1])
                : null;
            ushort count = rawGeoKeyDirectory[i + 2];
            ushort offset = rawGeoKeyDirectory[i + 3];

            object? value = null;
            if (location is null)
            {
                value = offset;
            }
            else
            {
                value = fileDirectory[location]; // TODO: could throw, error out if so.
                if (value is null)
                {
                    throw new GeoTiffException($"Could not get value of geoKey '{key}'");
                }

                value = ((Tag)value).Value;
                if (value is string)
                {
                    string? cast = (string)value;
                    value = cast.JSSubString(offset, offset + count - 1);
                }
                else if (value is List<object>)
                {
                    // value = value.subarray(offset, offset + count);
                    if (count == 1)
                    {
                        value = ((List<object>)value).First();
                    }
                }
                else
                {
                    throw new NotImplementedException("Unsupported tag type");
                }
            }

            geoKeyDirectory[key] = value;
        }

        return geoKeyDirectory;
    }

    protected internal async Task<ImageFileDirectory> ParseFileDirectoryAtAsync(int offset)
    {
        int entrySize = _bigTiff ? 20 : 12;
        int offsetSize = _bigTiff ? 8 : 2;

        DataSlice? dataSlice = await GetSliceAsync(offset);

        int numDirEntries = _bigTiff
            ? (int)dataSlice.ReadUInt64(offset)
            : dataSlice.ReadUInt16(offset);

        // Ensure the slice covers the whole IFD
        int byteSize = ((int)numDirEntries * (int)entrySize) + (_bigTiff ? 16 : 6);
        if (!dataSlice.Covers(offset, byteSize))
        {
            dataSlice = await GetSliceAsync(offset, byteSize);
        }

        var fileDirectory = new Dictionary<string, Tag>();
        var rawFileDirectory = new Dictionary<int, object>();

        int i = offset + (_bigTiff ? 8 : 2);
        for (long entryCount = 0; entryCount < numDirEntries; i += entrySize, ++entryCount)
        {
            ushort fieldTagId = dataSlice.ReadUInt16(i);
            ushort fieldType = dataSlice.ReadUInt16(i + 2);
            int typeCount = _bigTiff
                ? (int)dataSlice.ReadUInt64(i + 4)
                : (int)dataSlice.ReadUInt32(i + 4);

            GeotiffGetValuesResult fieldValues;
            object value;
            int fieldTypeLength = FieldTypes.GetFieldTypeLength(fieldType);
            GeotiffFieldDataType fieldTypeName = FieldTypes.FieldTypeLookup[fieldType];
            long valueOffset = i + (_bigTiff ? 12 : 8);
            // Check if the value is directly encoded or refers to another byte range
            if (fieldTypeLength * typeCount <= (_bigTiff ? 8 : 4))
            {
                fieldValues = dataSlice.GetValues(fieldType, typeCount, (int)valueOffset);
            }
            else
            {
                long actualOffset = dataSlice.ReadOffset((int)valueOffset);
                long length = FieldTypes.GetFieldTypeLength(fieldType) * typeCount;

                if (dataSlice.Covers((int)actualOffset, (int)length))
                {
                    fieldValues = dataSlice.GetValues(fieldType, typeCount, (int)actualOffset);
                }
                else
                {
                    DataSlice? fieldDataSlice = await GetSliceAsync((int)actualOffset, (int)length);
                    fieldValues = fieldDataSlice.GetValues(fieldType, typeCount, (int)actualOffset);
                }
            }

            // Unpack single values from the array
            if ((typeCount == 1 && !FieldTypes.ArrayTypeFields.Contains(fieldTagId)
                                && !(fieldTypeName == GeotiffFieldDataType.SRATIONAL)) || fieldTypeName == GeotiffFieldDataType.ASCII)
            {
                value = fieldValues.GetFirstElement();
                // value = (fieldValues as Array)?[0] ?? fieldValues;
            }
            else
            {
                if (fieldTypeName == GeotiffFieldDataType.SRATIONAL)
                {
                    throw new NotImplementedException($"SRationals not supported: {fieldTypeName}"); // TODO: Is this true anymore?
                }

                value = fieldValues.GetListOfElements();
            }

            // Write the tag's value to the file directory
            if (FieldTypes.FieldTags.TryGetByKey(fieldTagId, out string tagName))
            {
                fileDirectory[tagName] = new Tag(fieldTagId, tagName, fieldTypeName, value, false);
                rawFileDirectory[fieldTagId] = new Tag(fieldTagId, tagName, fieldTypeName, value, false);
            }
            else
            {
                rawFileDirectory[fieldTagId] = new Tag(fieldTagId, null, fieldTypeName, value, false);
            }
        }

        Dictionary<string, object>? geoKeyDirectory = ParseGeoKeyDirectory(fileDirectory);
        int nextIFDByteOffset = dataSlice.ReadOffset(
            offset + offsetSize + (entrySize * numDirEntries)
        );

        return new ImageFileDirectory(
            fileDirectory,
            rawFileDirectory,
            geoKeyDirectory,
            nextIFDByteOffset
        );
    }

    private SparseList<ImageFileDirectory> ImageFileDirectories = new();
    
    protected internal async Task<ImageFileDirectory> RequestIFDAsync(int index)
    {
        if (ImageFileDirectories[index] is not null)
        {
            return ImageFileDirectories[index];
        }

        if (index == 0)
        {
            ImageFileDirectory? result = await ParseFileDirectoryAtAsync((int)FirstIFDOffset);// TODO: this is a narrowing conversion
            ImageFileDirectories.Add(index, result);
            return result;
        }

        ImageFileDirectory? currentIFD = ImageFileDirectories[index - 1];
        if (currentIFD.NextIFDByteOffset == 0)
        {
            throw new GeoTiffImageIndexError(index);
        }

        ImageFileDirectory? result2 = await ParseFileDirectoryAtAsync(currentIFD.NextIFDByteOffset);
        ImageFileDirectories.Add(index, result2);
        return result2;
    }

    /// <summary>
    /// Test for the presence of overviews. This is denoted by GDAL as the first dataset being the largest, with
    /// all subsequent images being progressively smaller and smaller. However, note that this is not actually standard,
    /// so you may encounter datasets that do not conform to this pattern and may just happen to order their subdatasets
    /// like this. This method is therefore not foolproof.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> HasOverviewsAsync()
    {
        var imageCount = await this.GetImageCountAsync();
        if (imageCount < 2)
        {
            return false;
        }

        var currentImage = await this.GetImageAsync();
        for (int i = 1; i < imageCount; i++)
        {
            var possibleOverview = await this.GetImageAsync(i);
            if (currentImage.GetHeight() < possibleOverview.GetHeight() && currentImage.GetWidth() < possibleOverview.GetWidth())
            {
                return false;
            }

            currentImage = possibleOverview;
        }
        
        return true;
    }
    
    public virtual async Task<int> GetImageCountAsync()
    {
        if (this.finalImageCount is not null)
        {
            return (int)this.finalImageCount;
        }
        int index = 0;
        // loop until we run out of IFDs
        bool hasNext = true;
        while (hasNext)
        {
            try
            {
                await RequestIFDAsync(index);
                ++index;
            }
            catch (GeoTiffImageIndexError e) // TODO: bad, using exception for control flow. Diverge from geotiff.js here.
            {
                hasNext = false;
                this.finalImageCount = index;
            }
            catch
            {
                throw;
            }
        }
        
        return index;
    }


    public virtual async Task<GeoTiffImage> GetImageAsync(int index = 0)
    {
        if (ImageFileDirectories[index] is null)
        {
            int i = 0;
            while (i < index)
            {
                await RequestIFDAsync(i); // populate cache
                i++;
            }
        }

        ImageFileDirectory? ifd = await RequestIFDAsync(index);
        return new GeoTiffImage(
            ifd, IsLittleEndian, false, Source
        );
    }
}