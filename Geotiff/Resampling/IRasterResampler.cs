namespace Geotiff.Resampling;

public interface IRasterResampler
{
    public Raster Resample(Raster raster, int outWidth, int outHeight);
}