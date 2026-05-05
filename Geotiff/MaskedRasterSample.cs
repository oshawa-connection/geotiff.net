namespace Geotiff;

public class MaskedRasterSample
{
    public ulong Height { get; set; }
    public ulong Width { get; set; }
    private readonly RasterSample mainImageRasterSample;
    private readonly RasterSample? maskedSample;
    
    public MaskedRasterSample(RasterSample mainImageRasterSample, RasterSample? maskedSample)
    {
        this.mainImageRasterSample = mainImageRasterSample;
        this.maskedSample = maskedSample;
        Height = mainImageRasterSample.Height;
        Width = mainImageRasterSample.Width;
    }

    public MaskedSampleValue<double>[] GetAsDoubleArray()
    {
        var mainImageDoubleArray = this.mainImageRasterSample.GetAsDoubleArray();
        var maskedSampleDoubleArray = this.maskedSample.GetByteArray();

        return mainImageDoubleArray
            .Zip(maskedSampleDoubleArray, (a, b) => new MaskedSampleValue<double>(a, b != MaskedGeoTiffReader.EXTERNAL_MASK_YES_DATA_VALUE && b != MaskedGeoTiffReader.INTERNAL_MASK_YES_DATA_VALUE))
            .ToArray();
    }
    
    
    /// <summary>
    /// Rearranges the data into a 2D array indexed by result[pixelRow, pixelColumn] (y, x)
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private T[,] To2DArray<T>(T[] array)
    {
        if ((ulong)array.Length != Height * Width)
        {
            throw new InvalidOperationException("RawArrayData length does not match Height * Width.");
        }

        var result = new T[Height, Width];

        for (uint row = 0; row < Height; row++)
        {
            for (uint col = 0; col < Width; col++)
            {
                result[row, col] = array[row * Width + col];
            }
        }

        return result;
    }
    
    public MaskedSampleValue<double>[,] GetAs2DDoubleArray()
    {
        var doubles = this.GetAsDoubleArray();
        return this.To2DArray(doubles);
    }
}