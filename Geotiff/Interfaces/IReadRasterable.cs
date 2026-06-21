namespace Geotiff.Interfaces;

public interface IReadRasterable
{
    public Task<Raster> ReadRasterAsync(
        ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null);
    
    public Task<Raster> ReadRasterMaskedAsync(
        ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null);
    
    public Task<Raster> ReadRasterBoundingBoxAsync(BoundingBox boundingBox,
        IEnumerable<int>? sampleSelection = null, CancellationToken? cancellationToken = null);

    public Task<Raster> ReadRasterMaskedBoundingBoxAsync(BoundingBox boundingBox,
        IEnumerable<int>? sampleSelection = null, CancellationToken? cancellationToken = null);
}