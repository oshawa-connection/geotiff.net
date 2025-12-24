namespace Geotiff;

public interface IMaskedGeoTIFFReadResult<T> where T: struct
{
    public MaskedSampleReadResult<T> GetSampleResultAt(int sampleIndex);
}

public class MaskBandGeoTIFFReadResult<T> : IMaskedGeoTIFFReadResult<T> where T : struct
{
    private byte[] mask;
    /// <summary>
    /// A list of arrays of T. Each array represents a single sample.
    /// </summary>
    private IEnumerable<T[]> SampleData;
    private uint Width;
    private uint Height;
    private GeoTiffImage parentImage;
    
    public MaskBandGeoTIFFReadResult(byte[] mask, IEnumerable<T[]> sampleData, uint Width, uint Height,
        GeoTiffImage parentImage)
    {
        this.mask = mask;
        this.SampleData = sampleData;
        this.Width = Width;
        this.Height = Height;
        this.parentImage = parentImage;
    }
    
    public MaskedSampleReadResult<T> GetSampleResultAt(int sampleIndex)
    {
        List<MaskedValue<T>> maskedValues = new();
        var sampleData = this.SampleData.ElementAt(sampleIndex);
        for (var i = 0; i < mask.Length; i++)
        {
            var currentMaskValue = mask[i];
            var currentSampleResult = sampleData[i];
            maskedValues.Add(new MaskedValue<T>(currentSampleResult, currentMaskValue == 0));
        }
        return new MaskedSampleReadResult<T>(maskedValues, Width, Height, parentImage);
    }
}

public class MaskedSampleReadResult<T>(IEnumerable<MaskedValue<T>> maskedValues, uint Width, uint Height, GeoTiffImage parentImage)
{
    public IEnumerable<MaskedValue<T>> MaskedValues { get; set; } = maskedValues;
    /// <summary>
    /// This rearranges the data into a 2D array, indexed by result[pixelColumn, pixelRow] (x, y)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public MaskedValue<T>[,] To2DArray()
    {
        if (maskedValues.Count() != Height * Width)
        {
            throw new InvalidOperationException("RawArrayData length does not match Height * Width.");    
        }

        var result = new MaskedValue<T>[Width, Height];
        for (uint col = 0; col < Width; col++)
        {
            for (uint row = 0; row < Height; row++)
            {
                result[col, row] = maskedValues.ElementAt((int)(row * Width + col));
            }
        }
        return result;
    }
}

public class MaskedValue<T>(T value, bool masked)
{
    public T Value { get; } = value;
    public bool Masked => masked;
}