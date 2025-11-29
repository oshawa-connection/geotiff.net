using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;
using Geotiff.RemoteClients;

namespace Geotiff;

public class GeoTIFF
{
    private readonly BaseSource source;
    private readonly bool bigTiff;
    private readonly ulong firstIFDOffset;
    private readonly bool littleEndian;

    private static bool GetBomMarker(DataView dv)
    {
        ushort value = dv.getUint16(0, true);
        bool isLittleEndian = false;
        if (value == Constants.BOMLittleEndian)
        {
            isLittleEndian = true;
        }
        else if (value == Constants.BOMBigEndian)
        {
            isLittleEndian = false;
        }
        else
        {
            throw new Exception("Unrecognised Tiff BOM marker");
        }

        return isLittleEndian;
    }

    private static bool GetBigTiffMarker(DataView dv, bool isLittleEndian)
    {
        ushort isBigTiffValue = dv.getUint16(2, isLittleEndian);
        if (isBigTiffValue == 42)
        {
            return false;
        }

        if (isBigTiffValue == 43)
        {
            return true;
        }
        
        var offsetByteSize = dv.getUint16(4, isLittleEndian);
        if (offsetByteSize != 8) {
            throw new Exception("Unsupported offset byte-size.");
        }

        throw new Exception("Invalid tiff magic number.");
    }
    
    public static ulong GetFirstIFDOffset(DataView dv, bool isLittleEndian, bool isBigTiff)
    {
        // This is used to 
        return isBigTiff
            ? dv.getUint64(8, isLittleEndian)
            : dv.getUint32(4, isLittleEndian);
    }
    
    /// <summary>
    /// This is temporary for development purposes only.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<GeoTIFF> FromRemoteClient(IGeotiffRemoteClient client)
    {
        var source = new RemoteSource(client, int.MaxValue, false);
        IEnumerable<ArrayBuffer>? slices = await source.Fetch(new Slice[] { new(0, 1024) });

        var dv = new DataView(slices.First());
        ushort value = dv.getUint16(0, true);
        bool isLittleEndian = GetBomMarker(dv);

        bool isBigTiff = GetBigTiffMarker(dv, isLittleEndian);

        var firstIDFOffset= GetFirstIFDOffset(dv, isLittleEndian, isBigTiff);
        return new GeoTIFF(source, isLittleEndian, isBigTiff, firstIDFOffset);
    }


    

    public static async Task<GeoTIFF> FromStream(Stream stream)
    {
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        byte[]? arr = memoryStream.ToArray();
        var dv = new DataView(arr);
        ushort value = dv.getUint16(0, true);
        bool isLittleEndian = GetBomMarker(dv);

        bool isBigTiff = GetBigTiffMarker(dv, isLittleEndian);
        
        var firstIDFOffset= GetFirstIFDOffset(dv, isLittleEndian, isBigTiff);
        memoryStream.Position = 0;
        var source = new FileSource(memoryStream);
        return new GeoTIFF(source, isLittleEndian, isBigTiff, firstIDFOffset);
    }


    private GeoTIFF(BaseSource source, bool littleEndian, bool bigTiff, ulong firstIFDOffset)
    {
        this.source = source;
        this.littleEndian = littleEndian;
        this.bigTiff = bigTiff;
        this.firstIFDOffset = firstIFDOffset;
    }

    public async Task<DataSlice> GetSlice(int offset, int? size = null)
    {
        int fallbackSize = bigTiff ? 4048 : 1024;
        int sizeToUse = size is null ? fallbackSize : (int)size;
        var slice = new Slice(offset, sizeToUse, false);
        var slices = new List<Slice>() { slice };
        IEnumerable<ArrayBuffer>? results = await source.Fetch(slices);

        return new DataSlice(
            results.Single().GetAllBytes(), // TODO: Double check this.  
            offset,
            littleEndian,
            bigTiff
        );
    }


    private Dictionary<string, object>? ParseGeoKeyDirectory(Dictionary<string, object> fileDirectory)
    {
        bool rawGeoKeyDirectoryResult = fileDirectory.TryGetValue("GeoKeyDirectory", out object rawGeoKeyDirectoryObj);

        if (!rawGeoKeyDirectoryResult)
        {
            return null;
        }

        ushort[]? rawGeoKeyDirectory = ((List<object>)rawGeoKeyDirectoryObj).UnboxAll<ushort>().ToArray();

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
                    throw new Exception($"Could not get value of geoKey '{key}'");
                }

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

    private async Task<ImageFileDirectory> ParseFileDirectoryAt(int offset)
    {
        int entrySize = bigTiff ? 20 : 12;
        int offsetSize = bigTiff ? 8 : 2;

        DataSlice? dataSlice = await GetSlice(offset);

        int numDirEntries = bigTiff
            ? (int)dataSlice.ReadUInt64(offset)
            : dataSlice.ReadUInt16(offset);

        // Ensure the slice covers the whole IFD
        int byteSize = ((int)numDirEntries * (int)entrySize) + (bigTiff ? 16 : 6);
        if (!dataSlice.Covers(offset, byteSize))
        {
            dataSlice = await GetSlice(offset, byteSize);
        }

        var fileDirectory = new Dictionary<string, object>();
        var rawFileDirectory = new Dictionary<int, object>();

        int i = offset + (bigTiff ? 8 : 2);
        for (long entryCount = 0; entryCount < numDirEntries; i += entrySize, ++entryCount)
        {
            ushort fieldTag = dataSlice.ReadUInt16(i);
            ushort fieldType = dataSlice.ReadUInt16(i + 2);
            int typeCount = bigTiff
                ? (int)dataSlice.ReadUInt64(i + 4)
                : (int)dataSlice.ReadUInt32(i + 4);

            GeotiffGetValuesResult fieldValues;
            object value;
            int fieldTypeLength = FieldTypes.GetFieldTypeLength(fieldType);
            string? fieldTypeName = FieldTypes.FieldTypeLookup[fieldType];
            long valueOffset = i + (bigTiff ? 12 : 8);
            // Check if the value is directly encoded or refers to another byte range
            if (fieldTypeLength * typeCount <= (bigTiff ? 8 : 4))
            {
                fieldValues = dataSlice.getValues(fieldType, typeCount, (int)valueOffset);
            }
            else
            {
                long actualOffset = dataSlice.ReadOffset((int)valueOffset);
                long length = FieldTypes.GetFieldTypeLength(fieldType) * typeCount;

                if (dataSlice.Covers((int)actualOffset, (int)length))
                {
                    fieldValues = dataSlice.getValues(fieldType, typeCount, (int)actualOffset);
                }
                else
                {
                    DataSlice? fieldDataSlice = await GetSlice((int)actualOffset, (int)length);
                    fieldValues = fieldDataSlice.getValues(fieldType, typeCount, (int)actualOffset);
                }
            }

            // Unpack single values from the array
            if ((typeCount == 1 && !FieldTypes.ArrayTypeFields.Contains(fieldTag)
                                && !(fieldTypeName == FieldTypes.SRATIONAL)) || fieldTypeName == FieldTypes.ASCII)
            {
                value = fieldValues.GetFirstElement();
                // value = (fieldValues as Array)?[0] ?? fieldValues;
            }
            else
            {
                if (fieldTypeName == FieldTypes.SRATIONAL)
                {
                    throw new NotImplementedException($"SRationals not supported: {fieldTypeName}");
                }

                value = fieldValues.GetListOfElements();
            }

            // Write the tag's value to the file directory
            if (FieldTypes.FieldTags.TryGetByKey(fieldTag, out string tagName))
            {
                fileDirectory[tagName] = value;
            }

            rawFileDirectory[fieldTag] = value;
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


    private async Task<ImageFileDirectory> RequestIFD(int index)
    {
        if (ImageFileDirectories[index] is not null)
        {
            return ImageFileDirectories[index];
        }

        if (index == 0)
        {
            ImageFileDirectory? result = await ParseFileDirectoryAt((int)firstIFDOffset);// TODO: this is a narrowing conversion
            ImageFileDirectories.Add(index, result);
            return result;
        }

        ImageFileDirectory? currentIFD = ImageFileDirectories[index - 1];
        if (currentIFD.NextIFDByteOffset == 0)
        {
            throw new GeoTIFFImageIndexError(index);
        }

        ImageFileDirectory? result2 = await ParseFileDirectoryAt(currentIFD.NextIFDByteOffset);
        ImageFileDirectories.Add(index, result2);
        return result2;
    }

    public async Task<int> GetImageCount()
    {
        int index = 0;
        // loop until we run out of IFDs
        bool hasNext = true;
        while (hasNext)
        {
            try
            {
                await RequestIFD(index);
                ++index;
            }
            catch (GeoTIFFImageIndexError e)
            {
                // TODO: handle exceptions here properly
                hasNext = false;
                // throw;
                // if (e instanceof GeoTIFFImageIndexError) {
                //     hasNext = false;
                // } else {
                //     throw e;
                // }
            }
            catch
            {
                throw;
            }
        }

        return index;
    }


    public async Task<GeoTiffImage> GetImage(int index = 0)
    {
        if (ImageFileDirectories[index] is null)
        {
            int i = 0;
            while (i < index)
            {
                await RequestIFD(i); // populate cache
                i++;
            }
        }

        ImageFileDirectory? ifd = await RequestIFD(index);
        return new GeoTiffImage(
            ifd, littleEndian, false, source
        );
    }
}