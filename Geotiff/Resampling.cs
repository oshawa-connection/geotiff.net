// using System;
// using System.Collections.Generic;
// using Geotiff;
// using Geotiff.JavaScriptCompatibility;
//
// public static class Resampling
// {
//     /**
//     * Resample the pixel interleaved input array using the selected resampling method.
//     * @param {TypedArray} valueArray The input array to resample
//     * @param {number} inWidth The width of the input rasters
//     * @param {number} inHeight The height of the input rasters
//     * @param {number} outWidth The desired width of the output rasters
//     * @param {number} outHeight The desired height of the output rasters
//     * @param {number} samples The number of samples per pixel for pixel
//     *                                 interleaved data
//     * @param {string} [method = 'nearest'] The desired resampling method
//     * @returns {TypedArray} The resampled rasters
//     */
//     // public static RasterSample resampleInterleaved(GeotiffGetValuesResult valueArray, int inWidth, int inHeight, int outWidth, int outHeight, int samples, string method = "nearest")
//     // {
//     //     switch (method.toLowerCase())
//     //     {
//     //         case "nearest":
//     //             return resampleNearestInterleaved(
//     //                 valueArray, inWidth, inHeight, outWidth, outHeight, samples,
//     //
//     //
//     //             );
//     //         case "bilinear":
//     //         case "linear":
//     //             return resampleBilinearInterleaved(
//     //                 valueArray, inWidth, inHeight, outWidth, outHeight, samples,
//     //
//     //
//     //             );
//     //         default:
//     //             throw new Exception($"Unsupported resampling method: '{method}'");
//     //     }
//     // }
//
//     /**
//      * @module resample
//      */
//
//     private static T[] CopyNewSize<T>(int width, int height, int samplesPerPixel = 1)
//     {
//         return new T[width * height * samplesPerPixel];
//     }
//
//     /**
//      * Resample the input arrays using nearest neighbor value selection.
//      * @param {TypedArray[]} valueArrays The input arrays to resample
//      * @param {number} inWidth The width of the input rasters
//      * @param {number} inHeight The height of the input rasters
//      * @param {number} outWidth The desired width of the output rasters
//      * @param {number} outHeight The desired height of the output rasters
//      * @returns {TypedArray[]} The resampled rasters
//      */
//     // public static GeotiffGetValuesResult resampleNearest(List<GeotiffGetValuesResult> valueArrays, int inWidth, int inHeight, int outWidth, int outHeight)
//     // {
//     //     var relX = inWidth / outWidth;
//     //     var relY = inHeight / outHeight;
//     //     return valueArrays.map((array) =>
//     //     {
//     //         var newArray = copyNewSize(array, outWidth, outHeight);
//     //         for (var y = 0; y < outHeight; ++y)
//     //         {
//     //             var cy = Math.min(Math.round(relY * y), inHeight - 1);
//     //             for (var x = 0; x < outWidth; ++x)
//     //             {
//     //                 var cx = Math.min(Math.round(relX * x), inWidth - 1);
//     //                 var value = array[(cy * inWidth) + cx];
//     //                 newArray[(y * outWidth) + x] = value;
//     //             }
//     //         }
//     //         return newArray;
//     //     });
//     // }
//
//     // simple linear interpolation, code from:
//     // https://en.wikipedia.org/wiki/Linear_interpolation#Programming_language_support
//     private static double lerpDouble(double v0, double v1, double t)
//     {
//         return ((1 - t) * v0) + (t * v1);
//     }
//     
//     public static T[] resampleBilinear<T>(T[] array, int inWidth, int inHeight, int outWidth, int outHeight, Func<T,T,double, T> lerpFunc)
//     {
//         var relX = (double)inWidth / (double)outWidth;
//         var relY = (double)inHeight / (double)outHeight;
//         var newArray = CopyNewSize<T>(outWidth, outHeight);
//         for (var y = 0; y < outHeight; ++y)
//         {
//             var rawY = relY * y;
//
//             var yl = (int)Math.Floor(rawY);
//             var yh = (int)Math.Min(Math.Ceiling(rawY), (inHeight - 1));
//
//             for (var x = 0; x < outWidth; ++x)
//             {
//                 var rawX = relX * x;
//                 var tx = rawX % 1;
//                 
//                 var xl = (int)Math.Floor(rawX);
//                 var xh = (int)Math.Min(Math.Ceiling(rawX), (inWidth - 1));
//
//                 var ll = array[(yl * inWidth) + xl];
//                 var hl = array[(yl * inWidth) + xh];
//                 var lh = array[(yh * inWidth) + xl];
//                 var hh = array[(yh * inWidth) + xh];
//
//                 var lhs = lerpFunc(ll, hl, tx);
//                 var rhs = lerpFunc(lh, hh, tx);
//
//                 var value = lerpFunc(lhs, rhs, rawY % 1);
//                 
//                 newArray[(y * outWidth) + x] = value;
//             }
//         }
//         return newArray;
//     }
//     
//     public static Raster Resample(Raster raster, int inWidth, int inHeight, int outWidth, int outHeight, string method = "bilinear")
//     {
//         var resampled = new SparseList<RasterSample>();
//         switch (method.ToLower())
//         {
//             case "nearest":
//                 throw new NotImplementedException();
//                 // return resampleNearest(valueArrays, inWidth, inHeight, outWidth, outHeight);
//             case "bilinear":
//             case "linear":
//                 
//                 var sampleIndices = raster.ListSampleIndices();
//                 foreach (var sampleIndex in sampleIndices)
//                 {
//                     var currentSample = raster.GetSampleAt(sampleIndex);
//                     switch (currentSample.SampleType)
//                     {
//                         // case GeotiffSampleDataType.UInt8:
//                         //     
//                         //     break;
//                         // case GeotiffSampleDataType.Int8:
//                         //     break;
//                         // case GeotiffSampleDataType.Int16:
//                         //     break;
//                         // case GeotiffSampleDataType.UInt16:
//                         //     break;
//                         // case GeotiffSampleDataType.UInt32:
//                         //     break;
//                         // case GeotiffSampleDataType.UInt64:
//                         //     break;
//                         // case GeotiffSampleDataType.Int32:
//                         //     break;
//                         // case GeotiffSampleDataType.Float32:
//                         //     break;
//                         case GeotiffSampleDataType.Double:
//                             var arr = currentSample.GetDoubleArray();
//                             var resampledArr = resampleBilinear(arr, inWidth, inHeight, outWidth, outHeight, lerpDouble);
//                             resampled.Add(sampleIndex, new RasterSample((uint)outWidth, (uint)outHeight, null, resampledArr));
//                             break;
//                         default:
//                             throw new ArgumentOutOfRangeException();
//                     }
//                     
//                 }
//                 break;
//             default:
//                 throw new Exception($"Unsupported resampling method: '{method}'");
//         }
//
//         return new Raster(resampled, (uint)outWidth, (uint)outHeight, null);
//     }
//
//     /**
//      * Resample the pixel interleaved input array using nearest neighbor value selection.
//      * @param {TypedArray} valueArrays The input arrays to resample
//      * @param {number} inWidth The width of the input rasters
//      * @param {number} inHeight The height of the input rasters
//      * @param {number} outWidth The desired width of the output rasters
//      * @param {number} outHeight The desired height of the output rasters
//      * @param {number} samples The number of samples per pixel for pixel
//      *                         interleaved data
//      * @returns {TypedArray} The resampled raster
//      */
//     // public static GeotiffGetValuesResult resampleNearestInterleaved(
//     //   GeotiffGetValuesResult valueArray, int inWidth, int inHeight, int outWidth, int outHeight, int samples)
//     // {
//     //     var relX = inWidth / outWidth;
//     //     var relY = inHeight / outHeight;
//     //
//     //     var newArray = copyNewSize(valueArray, outWidth, outHeight, samples);
//     //     for (var y = 0; y < outHeight; ++y)
//     //     {
//     //         var cy = Math.min(Math.round(relY * y), inHeight - 1);
//     //         for (var x = 0; x < outWidth; ++x)
//     //         {
//     //             var cx = Math.min(Math.round(relX * x), inWidth - 1);
//     //             for (var i = 0; i < samples; ++i)
//     //             {
//     //                 var value = valueArray[(cy * inWidth * samples) + (cx * samples) + i];
//     //                 newArray[(y * outWidth * samples) + (x * samples) + i] = value;
//     //             }
//     //         }
//     //     }
//     //     return newArray;
//     // }
//
//     /**
//      * Resample the pixel interleaved input array using bilinear interpolation.
//      * @param {TypedArray} valueArrays The input arrays to resample
//      * @param {number} inWidth The width of the input rasters
//      * @param {number} inHeight The height of the input rasters
//      * @param {number} outWidth The desired width of the output rasters
//      * @param {number} outHeight The desired height of the output rasters
//      * @param {number} samples The number of samples per pixel for pixel
//      *                         interleaved data
//      * @returns {TypedArray} The resampled raster
//      */
//     // public static GeotiffGetValuesResult resampleBilinearInterleaved(
//     //   GeotiffGetValuesResult valueArray, int inWidth, int inHeight, int outWidth, int outHeight, GeotiffGetValuesResult samples)
//     // {
//     //     var relX = inWidth / outWidth;
//     //     var relY = inHeight / outHeight;
//     //     var newArray = copyNewSize(valueArray, outWidth, outHeight, samples);
//     //     for (var y = 0; y < outHeight; ++y)
//     //     {
//     //         var rawY = relY * y;
//     //
//     //         var yl = Math.floor(rawY);
//     //         var yh = Math.min(Math.ceil(rawY), (inHeight - 1));
//     //
//     //         for (var x = 0; x < outWidth; ++x)
//     //         {
//     //             var rawX = relX * x;
//     //             var tx = rawX % 1;
//     //
//     //             var xl = Math.floor(rawX);
//     //             var xh = Math.min(Math.ceil(rawX), (inWidth - 1));
//     //
//     //             for (var i = 0; i < samples; ++i)
//     //             {
//     //                 var ll = valueArray[(yl * inWidth * samples) + (xl * samples) + i];
//     //                 var hl = valueArray[(yl * inWidth * samples) + (xh * samples) + i];
//     //                 var lh = valueArray[(yh * inWidth * samples) + (xl * samples) + i];
//     //                 var hh = valueArray[(yh * inWidth * samples) + (xh * samples) + i];
//     //
//     //                 var value = lerp(
//     //                   lerp(ll, hl, tx),
//     //                   lerp(lh, hh, tx),
//     //                   rawY % 1
//     //                 );
//     //                 newArray[(y * outWidth * samples) + (x * samples) + i] = value;
//     //             }
//     //         }
//     //     }
//     //     return newArray;
//     // }
//
//     /**
//      * Resample the pixel interleaved input array using the selected resampling method.
//      * @param {TypedArray} valueArray The input array to resample
//      * @param {number} inWidth The width of the input rasters
//      * @param {number} inHeight The height of the input rasters
//      * @param {number} outWidth The desired width of the output rasters
//      * @param {number} outHeight The desired height of the output rasters
//      * @param {number} samples The number of samples per pixel for pixel
//      *                                 interleaved data
//      * @param {string} [method = 'nearest'] The desired resampling method
//      * @returns {TypedArray} The resampled rasters
//      */
//     // public static GeotiffGetValuesResult resampleInterleaved(GeotiffGetValuesResult valueArray, int inWidth, int inHeight, int outWidth, int outHeight, int samples, string method = "nearest")
//     // {
//     //     switch (method.toLowerCase())
//     //     {
//     //         case "nearest":
//     //             return resampleNearestInterleaved(
//     //               valueArray, inWidth, inHeight, outWidth, outHeight, samples
//     //
//     //
//     //             );
//     //         case "bilinear":
//     //         case "linear":
//     //             return resampleBilinearInterleaved(
//     //               valueArray, inWidth, inHeight, outWidth, outHeight, samples
//     //
//     //
//     //             );
//     //         default:
//     //             throw new Exception($"Unsupported resampling method: '{method}'");
//     //     }
//     // }
// }
//
