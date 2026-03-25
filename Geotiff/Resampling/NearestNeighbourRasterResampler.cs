namespace Geotiff.Resampling;

/// <summary>
/// This resampling method preserves your original data type i.e. doubles stay as doubles, bytes and bytes etc.
/// Note that this does not account for masked pixels.
/// </summary>
public class NearestNeighbourRasterResampler : RasterResamplerBaseClass
{
    private T[] CopyNewSize<T>(int width, int height)
    {
        return new T[width * height];
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
    
    public override Raster Resample(Raster raster, int outWidth, int outHeight)
    {
         var resampled = new SparseList<RasterSample>();
         var sampleIndices = raster.ListSampleIndices();
         foreach (var sampleIndex in sampleIndices)
         {
             var currentSample = raster.SampleAt(sampleIndex);
             int[] arr;
             switch (currentSample.SampleType)
             {
                 case GeotiffSampleDataType.UInt8:
                     var uint8Array = currentSample.GetByteArray();
                     var resampleduintArray = resampleNN(uint8Array, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampleduintArray));
                     break;
                 case GeotiffSampleDataType.Int8:
                     var int8Array = currentSample.GetSByteArray();
                     var resampledint8Array = resampleNN(int8Array, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledint8Array));
                     break;
                 case GeotiffSampleDataType.Int16:
                     var shortArray = currentSample.GetShortArray();
                     var resampledShortArray = resampleNN(shortArray, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledShortArray));
                     break;
                 case GeotiffSampleDataType.UInt16:
                     var uShortArray = currentSample.GetUShortArray();
                     var resampledUShortArray = resampleNN(uShortArray, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledUShortArray));
                     break;
                 case GeotiffSampleDataType.UInt32:
                     var uInt32Array = currentSample.GetUIntArray();
                     var resampledUIntArray = resampleNN(uInt32Array, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledUIntArray));
                     break;
                 case GeotiffSampleDataType.UInt64:
                     var uInt64Array = currentSample.GetUIntArray();
                     var resampledUInt64Array = resampleNN(uInt64Array, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledUInt64Array));
                     break;
                 case GeotiffSampleDataType.Int32:
                     var int32Array = currentSample.GetIntArray();
                     var resampledInt32Array = resampleNN(int32Array, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledInt32Array));
                     break;
                 case GeotiffSampleDataType.Float32:
                     var float32Array = currentSample.GetFloatArray();
                     var resampledFloat32Array = resampleNN(float32Array, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledFloat32Array));
                     break;
                 case GeotiffSampleDataType.Float64:
                     var doubleArray = currentSample.GetDoubleArray();
                     var resampledDoubleArray = resampleNN(doubleArray, (int)raster.Width, (int)raster.Height, outWidth, outHeight);
                     resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, raster.ParentImage, resampledDoubleArray));
                     break;
                 default:
                     throw new ArgumentOutOfRangeException();
             }
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