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
    private double? noDataValue;
    private MaskedGeoTiffReader parentImage;
    public MaskedRaster(Raster mainRaster, Raster maskRaster, AffineTransformation? affine, ulong width, ulong height, MaskedGeoTiffReader parentImage, MaskedGeoTiffStrategy maskStrategy, double? noDataValue = null)
    {
        this.mainRaster = mainRaster;
        this.maskRaster = maskRaster;
        this._strategy = maskStrategy;
        this.Height = height;
        this.Width = width;
        this.affine = affine;
        this.parentImage = parentImage;
        this.noDataValue = noDataValue;
    }
    
    public MaskedRasterSample GetSampleAt(int index)
    {
        if (this._strategy == MaskedGeoTiffStrategy.INTERNAL_MASK)
        {
            // +1 here because the first sample is the mask band TODO: check this 
            var mainSample = this.mainRaster.GetSampleAt(index + 1);
            var maskedSample = this.mainRaster.GetSampleAt(0);
            return new MaskedRasterSample(mainSample, maskedSample, this._strategy);
        } 
        if (this._strategy == MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE)
        {
            var mainSample = this.mainRaster.GetSampleAt(index);
        
            return new MaskedRasterSample(mainSample, this.maskRaster.GetSampleAt(0), this._strategy);    
        }

        if (this._strategy == MaskedGeoTiffStrategy.ALPHA_BAND)
        {
            var mainSample = this.mainRaster.GetSampleAt(index);
            return new MaskedRasterSample(mainSample, this.maskRaster.GetSampleAt(3), this._strategy); // Alpha channel RGBA
        }

        if (this._strategy == MaskedGeoTiffStrategy.NO_DATA_VALUE)
        {
            var mainSample = this.mainRaster.GetSampleAt(index);
            return new MaskedRasterSample(mainSample, null, this._strategy, noDataValue);
        }

        throw new GeoTiffException("An error occurred while reading the masked raster");
    }
}