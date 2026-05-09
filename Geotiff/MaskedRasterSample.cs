using Geotiff.Exceptions;

namespace Geotiff;

public class MaskedRasterSample
{
    public ulong Height { get; set; }
    public ulong Width { get; set; }
    private readonly RasterSample mainImageRasterSample;
    private readonly RasterSample? maskedSample;
    private readonly MaskedGeoTiffStrategy strategy;
    private readonly double? noDataValue;
    public MaskedRasterSample(RasterSample mainImageRasterSample, RasterSample? maskedSample, MaskedGeoTiffStrategy strategy, double? noDataValue = null)
    {
        this.mainImageRasterSample = mainImageRasterSample;
        this.maskedSample = maskedSample;
        Height = mainImageRasterSample.Height;
        Width = mainImageRasterSample.Width;
        this.strategy = strategy;
        this.noDataValue = noDataValue;
    }

    public MaskedSampleValue<double>[] GetAsDoubleArray()
    {
        var mainImageDoubleArray = this.mainImageRasterSample.GetAsDoubleArray();
        

        if (this.strategy == MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE)
        {
            var maskedSampleDoubleArray = this.maskedSample.GetByteArray();
            return mainImageDoubleArray
                .Zip(maskedSampleDoubleArray, (a, b) => new MaskedSampleValue<double>(a, b != MaskedGeoTiffReader.EXTERNAL_MASK_YES_DATA_VALUE))
                .ToArray();            
        }
        
        if (this.strategy == MaskedGeoTiffStrategy.INTERNAL_MASK)
        {
            var maskedSampleDoubleArray = this.maskedSample.GetByteArray();
            return mainImageDoubleArray
                    .Zip(maskedSampleDoubleArray, (a, b) => new MaskedSampleValue<double>(a,  b != MaskedGeoTiffReader.INTERNAL_MASK_YES_DATA_VALUE))
                    .ToArray();      
        }
        
        if (this.strategy == MaskedGeoTiffStrategy.ALPHA_BAND)
        {
            var maskedSampleDoubleArray = this.maskedSample.GetByteArray();
            return mainImageDoubleArray
                .Zip(maskedSampleDoubleArray, (a, b) => new MaskedSampleValue<double>(a,  b == MaskedGeoTiffReader.ALPHA_BAND_NO_DATA))
                .ToArray();      
        }

        if (this.strategy == MaskedGeoTiffStrategy.NO_DATA_VALUE)
        {
            return mainImageDoubleArray.Select(d => new MaskedSampleValue<double>(d, d == noDataValue)).ToArray();
        }

        throw new GeoTiffException("Unrecognised masked data format");
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