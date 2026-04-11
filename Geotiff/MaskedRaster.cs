namespace Geotiff;

public class MaskedRaster
{
    private AffineTransformation? affine;
    public ulong Height { get; set; }
    public ulong Width { get; set; }
    private Raster mainRaster { get; set; }
    private Raster maskRaster { get; set; }
    public MaskedRaster(Raster mainRaster, Raster maskRaster, AffineTransformation? affine, ulong width, ulong height, MaskedGeoTiffReader parentImage)
    {
        this.mainRaster = mainRaster;
        this.maskRaster = maskRaster;
    }

    public MaskedRasterSample GetSampleAt(int index)
    {
        var mainSample = this.mainRaster.GetSampleAt(index);

        return new MaskedRasterSample(mainSample, this.maskRaster.GetSampleAt(0));
    }
}