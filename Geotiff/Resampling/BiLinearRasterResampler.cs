namespace Geotiff.Resampling;

/// <summary>
/// Your raster will be converted to a double after resampling.
/// Note that this does not account for masked pixels.
/// </summary>
public class BiLinearRasterResampler : RasterResamplerBaseClass
{
    private T[] CopyNewSize<T>(int width, int height)
    {
        return new T[width * height];
    }
    
    private double lerpDouble(double v0, double v1, double t)
    {
        return ((1 - t) * v0) + (t * v1);
    }
    
    private T[] resampleBilinear<T>(T[] array, int inWidth, int inHeight, int outWidth, int outHeight, Func<T,T,double, T> lerpFunc)
    {
        var relX = (double)inWidth / (double)outWidth;
        var relY = (double)inHeight / (double)outHeight;
        var newArray = CopyNewSize<T>(outWidth, outHeight);
        for (var y = 0; y < outHeight; ++y)
        {
            var rawY = relY * y;

            var yl = (int)Math.Floor(rawY);
            var yh = (int)Math.Min(Math.Ceiling(rawY), (inHeight - 1));

            for (var x = 0; x < outWidth; ++x)
            {
                var rawX = relX * x;
                var tx = rawX % 1;
                
                var xl = (int)Math.Floor(rawX);
                var xh = (int)Math.Min(Math.Ceiling(rawX), (inWidth - 1));

                var ll = array[(yl * inWidth) + xl];
                var hl = array[(yl * inWidth) + xh];
                var lh = array[(yh * inWidth) + xl];
                var hh = array[(yh * inWidth) + xh];

                var lhs = lerpFunc(ll, hl, tx);
                var rhs = lerpFunc(lh, hh, tx);

                var value = lerpFunc(lhs, rhs, rawY % 1);
                
                newArray[(y * outWidth) + x] = value;
            }
        }
        return newArray;
    }
    
    public override Raster Resample(Raster raster, int outWidth, int outHeight)
    {
        var resampled = new SparseList<RasterSample>();
        var sampleIndices = raster.ListSampleIndices();
        foreach (var sampleIndex in sampleIndices)
        {
            var currentSample = raster.GetSampleAt(sampleIndex);
            double[] arr;
            switch (currentSample.SampleType)
            {
                case GeotiffSampleDataType.Float64:
                    arr = currentSample.GetDoubleArray(); // prevent unnecessary conversion.
                    break;
                default:
                    // Otherwise just convert them - integers, float32s, everything!
                    arr = currentSample.GetAsDoubleArray();
                    break;
            }
            
            var resampledArr = resampleBilinear(arr, (int)raster.Width, (int)raster.Height, outWidth, outHeight, lerpDouble);
            resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledArr));
        }

        var affineToUse = raster.AffineTransformation; 
        
        if (affineToUse is not null)
        {
            var newRes = CalculateNewResolution(raster, outWidth, outHeight);
            affineToUse = affineToUse.Copy();
            affineToUse.SetResolution(newRes);
        }
        
        return new Raster(resampled, affineToUse, (uint)outWidth, (uint)outHeight, raster.ParentImage);
    }
}