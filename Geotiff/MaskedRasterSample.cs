namespace Geotiff;

public class MaskedRasterSample
{
    private readonly RasterSample mainImageRasterSample;
    private readonly RasterSample? maskedSample;
    
    public MaskedRasterSample(RasterSample mainImageRasterSample, RasterSample? maskedSample)
    {
        this.mainImageRasterSample = mainImageRasterSample;
        this.maskedSample = maskedSample;
    }

    public MaskedSampleValue<double>[] GetAsDoubleArray()
    {
        var mainImageDoubleArray = this.mainImageRasterSample.GetAsDoubleArray();
        var maskedSampleDoubleArray = this.maskedSample.GetByteArray();

        return mainImageDoubleArray
            .Zip(maskedSampleDoubleArray, (a, b) => new MaskedSampleValue<double>(a, b != MaskedGeoTiffReader.EXTERNAL_MASK_YES_DATA_VALUE && b != MaskedGeoTiffReader.INTERNAL_MASK_YES_DATA_VALUE))
            .ToArray();
    }
}