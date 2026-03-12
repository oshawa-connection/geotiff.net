namespace Geotiff.Resampling;

public abstract class RasterResamplerBaseClass
{
    public abstract Raster Resample(Raster raster, int outWidth, int outHeight);

    public VectorXYZ CalculateNewResolution(Raster raster, int outWidth, int outHeight)
    {
        var oldRes = raster.GetResolution();
        var oldHeight = raster.Height;
        var oldWidth = raster.Width;

        var xRes = (oldWidth * oldRes.X) / (double)outWidth;
        var yRes = (oldHeight * oldRes.Y) / (double)outHeight;

        return new VectorXYZ()
        {
            X = xRes,
            Y = yRes,
            Z = oldRes.Z
        };
    }
}