using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public class GeoTIFF
{
    private readonly BaseSource source;
    private readonly bool bigTiff;
    private readonly int firstIFDOffset;
    private readonly bool littleEndian;

    public async static Task<GeoTIFF> FromStream(Stream stream)
    {
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        var arr = memoryStream.ToArray();
        var dv = new DataView(arr);
        var value = dv.getUint16(0, true);
        var isLittleEndian = false;
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
        
        var isBigTiffValue = dv.getUint16(2, isLittleEndian);
        var firstIDFOffset = dv.getUint16(4, isLittleEndian);
        memoryStream.Position = 0;
        var source = new FileSource(memoryStream);
        return new GeoTIFF(source, isLittleEndian, false, firstIDFOffset);
    }
    
    
    private GeoTIFF(BaseSource source, bool littleEndian, bool bigTiff, int firstIFDOffset)
    {
        this.source = source;
        this.littleEndian = littleEndian;
        this.bigTiff = bigTiff;
        this.firstIFDOffset = firstIFDOffset;
    }
    
    public async Task<DataSlice> GetSlice(int offset, int? size = null) {
        var fallbackSize = this.bigTiff ? 4048 : 1024;
        var sizeToUse = size is null ? fallbackSize : (int)size;
        var slice = new Slice(offset, sizeToUse, false);
        var slices = new List<Slice>() { slice };
        var results = await this.source.Fetch(slices);
        
        return new DataSlice(
            results.Single(),
            offset,
            this.littleEndian,
            this.bigTiff
        );
    }


    public Dictionary<string, object>? ParseGeoKeyDirectory(Dictionary<string, object> fileDirectory)
    {
        var rawGeoKeyDirectoryResult = fileDirectory.TryGetValue("GeoKeyDirectory", out object rawGeoKeyDirectoryObj);
        
        if (!rawGeoKeyDirectoryResult) {
            return null;
        }

        var rawGeoKeyDirectory = ((List<object>)rawGeoKeyDirectoryObj).UnboxAll<ushort>().ToArray();
        
        Dictionary<string, object> geoKeyDirectory = new();
        for (var i = 4; i <= rawGeoKeyDirectory[3] * 4; i += 4)
        {
            string key = FieldTypes.GeoKeyNames.GetByKey(rawGeoKeyDirectory[i]);
            // string key = FieldTypes.GeoKeyNames[rawGeoKeyDirectory[i]];
            // TODO: try find a tif where this is 0. Not clear what value it should be, perhaps undefined in JS means array isn't set?
            var location = rawGeoKeyDirectory[i + 1] != 0 ? (FieldTypes.FieldTags.GetByKey(rawGeoKeyDirectory[i + 1])) : null;
            var count = rawGeoKeyDirectory[i + 2];
            var offset = rawGeoKeyDirectory[i + 3];
    
            object? value = null;
            if (location is null) {
                value = offset;
            } else {
                value = fileDirectory[location]; // TODO: could throw, error out if so.
                if (value is null)
                {
                    throw new Exception($"Could not get value of geoKey '{key}'");
                }
                if (value is string)
                {
                    var cast = (string)value;
                    value = cast.Substring(offset, offset + count - 1);
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
        int entrySize = this.bigTiff ? 20 : 12;
        int offsetSize = this.bigTiff ? 8 : 2;

        var dataSlice = await GetSlice(offset);
        
        int numDirEntries = this.bigTiff
            ? (int)dataSlice.ReadUInt64(offset)
            : dataSlice.ReadUInt16(offset);

        // Ensure the slice covers the whole IFD
        int byteSize = ((int)numDirEntries * (int)entrySize) + (this.bigTiff ? 16 : 6);
        if (!dataSlice.Covers(offset, byteSize))
        {
            dataSlice = await GetSlice(offset, byteSize);
        }

        var fileDirectory = new Dictionary<string, object>();
        var rawFileDirectory = new Dictionary<int, object>();

        int i = offset + (this.bigTiff ? 8 : 2);
        for (long entryCount = 0; entryCount < numDirEntries; i += entrySize, ++entryCount)
        {
            ushort fieldTag = dataSlice.ReadUInt16(i);
            ushort fieldType = dataSlice.ReadUInt16(i + 2);
            int typeCount = this.bigTiff
                ? (int)dataSlice.ReadUInt64(i + 4)
                : (int)dataSlice.ReadUInt32(i + 4);

            GeotiffGetValuesResult fieldValues;
            object value;
            int fieldTypeLength = FieldTypes.GetFieldTypeLength(fieldType);
            var fieldTypeName = FieldTypes.FieldTypeLookup[fieldType];
            long valueOffset = i + (this.bigTiff ? 12 : 8);
            // Check if the value is directly encoded or refers to another byte range
            if (fieldTypeLength * typeCount <= (this.bigTiff ? 8 : 4))
            {
                fieldValues = dataSlice.getValues(fieldType, typeCount, (int)valueOffset);
            }
            else
            {
                long actualOffset = dataSlice.ReadOffset((int)valueOffset);
                long length = FieldTypes.GetFieldTypeLength(fieldType) * typeCount;
            
                if (dataSlice.Covers((int)actualOffset, (int)length))
                {
                    fieldValues = dataSlice.getValues( fieldType, typeCount, (int)actualOffset);
                }
                else
                {
                    var fieldDataSlice = await GetSlice((int)actualOffset, (int)length);
                    fieldValues = fieldDataSlice.getValues(fieldType, typeCount,(int) actualOffset);
                }
            }
        
            // Unpack single values from the array
            if (typeCount == 1 && !FieldTypes.ArrayTypeFields.Contains(fieldTag)
                && !(fieldTypeName == FieldTypes.RATIONAL || fieldTypeName == FieldTypes.SRATIONAL) || fieldTypeName == FieldTypes.ASCII)
            {
                value = fieldValues.GetFirstElement();
                // value = (fieldValues as Array)?[0] ?? fieldValues;
            }
            else
            {
                if (fieldTypeName == FieldTypes.RATIONAL || fieldTypeName == FieldTypes.SRATIONAL)
                {
                    throw new NotImplementedException($"Rationals not supported: {fieldTypeName}");    
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
        
        var geoKeyDirectory = ParseGeoKeyDirectory(fileDirectory);
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
            var result = await this.ParseFileDirectoryAt(this.firstIFDOffset);
            ImageFileDirectories.Add(index, result);
            return result;
        }

        var currentIFD = ImageFileDirectories[index - 1];
        var result2 = await this.ParseFileDirectoryAt(currentIFD.NextIFDByteOffset);
        ImageFileDirectories.Add(index, result2);
        return result2;
    }
    
    public async Task<int> GetImageCount() {
        var index = 0;
        // loop until we run out of IFDs
        var hasNext = true;
        while (hasNext) {
            try {
                await this.RequestIFD(index);
                ++index;
            } catch (Exception e) { // TODO: handle exceptions here properly
                hasNext = false;
                // throw;
                // if (e instanceof GeoTIFFImageIndexError) {
                //     hasNext = false;
                // } else {
                //     throw e;
                // }
            }
        }
        return index;
    }
    
    
    public async Task<GeoTiffImage> GetImage(int index = 0) {
        
        if (this.ImageFileDirectories[index] is null)
        {
            var i = 0;
            while (i < index)
            {
                await this.RequestIFD(i); // populate cache
                i++;
            }
        }
        var ifd = await this.RequestIFD(index);
        return new GeoTiffImage(
            ifd, this.littleEndian, false, this.source
        );
    }
}