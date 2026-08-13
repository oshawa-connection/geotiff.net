namespace Geotiff.Masking;

public abstract class MaskStrategyABC
{
    /// <summary>
    /// Throw if invalid
    /// </summary>
    /// <param name="window"></param>
    /// <param name="sampleSelection"></param>
    /// <param name="cancellationToken"></param>
    public virtual void ValidateRasterReadArguments(ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null)
    {

    }

    public abstract Task SetMaskValues(GeoTiff parentFile,Raster mainReadResult, ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null);
}