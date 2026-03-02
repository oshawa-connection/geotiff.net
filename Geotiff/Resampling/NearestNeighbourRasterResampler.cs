namespace Geotiff.Resampling;

public class NearestNeighbourRasterResampler : IRasterResampler
{
    private T[] CopyNewSize<T>(int width, int height, int samplesPerPixel = 1)
    {
        return new T[width * height * samplesPerPixel];
    }

    public T[] resampleNN<T>(T[] array, int inWidth, int inHeight, int outWidth, int outHeight)
    {
        var relX = (double)inWidth / (double)outWidth;
        var relY = (double)inHeight / (double)outHeight;
        
        var newArray = CopyNewSize<T>(outWidth, outHeight);
        for (var y = 0; y < outHeight; ++y)
        {
            var cy = (int)Math.Min(Math.Round(relY * y), inHeight - 1);
            for (var x = 0; x < outWidth; ++x)
            {
                var cx = (int)Math.Min(Math.Round(relX * x), inWidth - 1);
                var value = array[(cy * inWidth) + cx];
                newArray[(y * outWidth) + x] = value;
            }
        }
        return newArray;
    }
    
    public Raster Resample(Raster raster, int outWidth, int outHeight)
    {
         var resampled = new SparseList<RasterSample>();
         var sampleIndices = raster.ListSampleIndices();
         foreach (var sampleIndex in sampleIndices)
         {
             var currentSample = raster.GetSampleAt(sampleIndex);
             int[] arr;
             switch (currentSample.SampleType)
             {
                 case GeotiffSampleDataType.Int32:
                     arr = currentSample.GetIntArray(); // prevent unnecessary conversion.
                     break;
                 default:
                     // Otherwise just convert them - integers, float32s, everything!
                     arr = currentSample.GetAsIntArray();
                     break;
             }
            
             var resampledArr = resampleNN(arr, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
             resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledArr));
         }

         return new Raster(resampled, (uint)outWidth, (uint)outHeight, raster.ParentImage);
    }
}