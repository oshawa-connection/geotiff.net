namespace Geotiff;

public class MaskedRaster: Raster
{
    public MaskedRaster(SparseList<RasterSample> sampleData, AffineTransformation? affine, ulong width, ulong height, GeoTiffImage parentImage) : base(sampleData, affine, width, height, parentImage)
    {
    }
}