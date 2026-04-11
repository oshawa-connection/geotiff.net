using Geotiff.Exceptions;

namespace Geotiff;

public class MaskedRaster
{
    private AffineTransformation? affine;
    public ulong Height { get; set; }
    public ulong Width { get; set; }
    private Raster mainRaster { get; set; }
    private Raster maskRaster { get; set; }
    private MaskedGeoTiffStrategy _strategy;
    public MaskedRaster(Raster mainRaster, Raster maskRaster, AffineTransformation? affine, ulong width, ulong height, MaskedGeoTiffReader parentImage)
    {
        this.mainRaster = mainRaster;
        this.maskRaster = maskRaster;
        this._strategy = MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE;
    }

    public MaskedRaster(Raster mainRaster)
    {
        this.mainRaster = mainRaster;
        this._strategy = MaskedGeoTiffStrategy.INTERNAL_ALPHA_BAND;
    }

    public MaskedRaster(Raster mainRaster, double noDataValue)
    {
        this.mainRaster = mainRaster;
        this._strategy = MaskedGeoTiffStrategy.NO_DATA_VALUE;
    }

    public MaskedRasterSample GetSampleAt(int index)
    {
        if (this._strategy == MaskedGeoTiffStrategy.INTERNAL_ALPHA_BAND)
        {
            // +1 here because the first sample is the mask band
            var mainSample = this.mainRaster.GetSampleAt(index + 1);
            var maskedSample = this.mainRaster.GetSampleAt(0);
            return new MaskedRasterSample(mainSample, maskedSample);
        } else if (this._strategy == MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE)
        {
            var mainSample = this.mainRaster.GetSampleAt(index);
        
            return new MaskedRasterSample(mainSample, this.maskRaster.GetSampleAt(0));    
        }

        throw new GeoTiffException("An error occurred while reading the masked raster");
    }
}