using Geotiff.Exceptions;
using System.Collections;

namespace Geotiff;

/// <summary>
/// Data structure for rasters, used for both reading and writing.
/// </summary>
/// <param name="sampleData"></param>
/// <param name="width"></param>
/// <param name="height"></param>
/// <param name="parentImage"></param>
public class Raster
{
    public Raster(SparseList<RasterSample> sampleData, uint width, uint height, GeoTiffImage parentImage)
    {
        this.Height = height;
        this.Width = width;
        this.SampleData = sampleData;
        this.ParentImage = parentImage;
    }
    
    public uint Height { get; set; }
    public uint Width { get; set; }
    public readonly GeoTiffImage ParentImage;
    /// <summary>
    /// A SparseList of samples. Samples are indexed by their index in the
    /// parent image. E.g. if you request samples 1 and 10, elements 1 and 10 will be set in this list and the
    /// rest of the samples won't be present.
    /// </summary>
    private SparseList<RasterSample> SampleData { get; set; }

    public int GetNumberOfSamples()
    {
        return this.SampleData.Count();
    }
    
    public IEnumerable<int> ListSampleIndices()
    {
        return this.SampleData.GetIndices();
    }

    public IEnumerable<RasterSample> GetSamples()
    {
        return this.SampleData.ToList();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sampleIndex">This is the index within the parent GeoTiff.</param>
    /// <returns></returns>
    public RasterSample GetSampleAt(int sampleIndex)
    {
        if (this.ListSampleIndices().Contains(sampleIndex) is false)
        {
            throw new GeoTiffException($"Sample with index {sampleIndex} was not found in the Raster.");
        }
        return this.SampleData[sampleIndex];
    }
}