namespace Geotiff;

public class GeoTIFFReadResult<T>(IEnumerable<T[]> sampleData, uint width, uint height, GeoTiffImage parentImage) where T : struct
{
    public uint Height { get; set; } = height;
    public uint Width { get; set; } = width;
    /// <summary>
    /// A list of arrays of T. Each array represents a single sample.
    /// TODO: Should this be a sparse list?
    /// </summary>
    public IEnumerable<T[]> SampleData { get; set; } = sampleData;
    private readonly GeoTiffImage ParentImage = parentImage;

    public SampleReadResult<T> GetSampleResultAt(int sampleIndex)
    {
        return new SampleReadResult<T>(this.SampleData.ElementAt(sampleIndex), width, height, parentImage);
    }
}