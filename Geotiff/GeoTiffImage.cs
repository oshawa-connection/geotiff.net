using System;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using Geotiff.Compression;
using Geotiff.JavaScriptCompatibility;
using Geotiff.Projection;

namespace Geotiff;

public class GeoTiffImage
{
  protected readonly ImageFileDirectory fileDirectory;
  private readonly bool littleEndian;
  private readonly bool cache;
  private readonly BaseSource source;
  private readonly Dictionary<int, ArrayBuffer> tiles;
  private readonly bool isTiled;
  private readonly ushort planarConfiguration;

  public GeoTiffImage(ImageFileDirectory fileDirectory, bool littleEndian, bool cache, BaseSource source)
  {
    this.fileDirectory = fileDirectory;
    this.littleEndian = littleEndian;
    this.tiles = cache ? new Dictionary<int, ArrayBuffer>() : null;

    this.isTiled = fileDirectory.FileDirectory.ContainsKey("StripOffsets") is false;
    var planarConfiguration = fileDirectory.GetFileDirectoryValue<ushort?>(FieldTypes.PlanarConfiguration);
    
    if (planarConfiguration is null)
    {
      this.planarConfiguration = 1;
    }
    else
    {
      this.planarConfiguration = (ushort)planarConfiguration;
    }
    
    // var planarConfiguration = fileDirectory.PlanarConfiguration;
    // this.planarConfiguration = (typeof planarConfiguration == 'undefined') ? 1 : planarConfiguration;
    if (this.planarConfiguration != 1 && this.planarConfiguration != 2) {
        throw new Exception("Invalid planar configuration.");
    }

    this.source = source;
  }


  public VectorXYZ GetOrigin()
  {
    var tiePoint = this.fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTiepoint);
    var modelTransformation = this.fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);

    if (tiePoint is not null && tiePoint.Count() == 6)
    {
      return new VectorXYZ()
      {
        X = tiePoint.ElementAt(3),
        Y = tiePoint.ElementAt(4),
        Z = tiePoint.ElementAt(5)
      };
    }
    if (modelTransformation is not null)
    {// TODO: check modeltransformation length

      return new VectorXYZ()
      {
        X = modelTransformation.ElementAt(3),
        Y = modelTransformation.ElementAt(7),
        Z = modelTransformation.ElementAt(11)
      };
    }
    throw new Exception("The image does not have an affine transformation.");
  }


  /// <summary>
  /// uint return type here is confirmed by geotiff spec
  /// </summary>
  /// <returns></returns>
  public uint GetWidth()
  {
    return this.fileDirectory.GetFileDirectoryValue<uint>(FieldTypes.ImageWidth);
  }

  /// <summary>
  /// uint return type here is confirmed by geotiff spec
  /// </summary>
  /// <returns></returns>
  public uint GetHeight()
  {
    return this.fileDirectory.GetFileDirectoryValue<uint>(FieldTypes.ImageLength);
  }

  public Tuple<double, double, double> GetResolution()
  {
    var modelPixelScaleR = this.fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelPixelScale);
    var modelTransformationR = this.fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);

    if (modelPixelScaleR is not null)
    {
      var modelPixelScale = modelPixelScaleR.ToArray();
      return new Tuple<double, double, double>(
          modelPixelScale[0],
          -modelPixelScale[1],
          modelPixelScale[2]);
    }
    if (modelTransformationR is not null)
    {
      var modelTransformation = modelTransformationR.ToArray();
      if (modelTransformation[1] == 0 && modelTransformation[4] == 0)
      {
        return new Tuple<double, double, double>(
            modelTransformation[0],
            -modelTransformation[5],
            modelTransformation[10]);
      }
      return new Tuple<double, double, double>(
          Math.Sqrt((modelTransformation[0] * modelTransformation[0])
                    + (modelTransformation[4] * modelTransformation[4])),
          -Math.Sqrt((modelTransformation[1] * modelTransformation[1])
                     + (modelTransformation[5] * modelTransformation[5])),
          modelTransformation[10]);
    }

    throw new Exception("The image does not have an affine transformation.");
  }


  /// <summary>
  /// Returns the image bounding box as an array of 4 values: min-x, min-y,
  /// max-x and max-y. When the image has no affine transformation, then an
  /// exception is thrown.
  /// 
  /// </summary>
  /// <param name="tilegrid">If true return extent for a tilegrid without adjustment for ModelTransformation.</param>
  /// <returns>The bounding box</returns>
  public BoundingBox GetBoundingBox(bool tilegrid = false)
  {
    var height = this.GetHeight();
    var width = this.GetWidth();
    var modelTransformationList = this.fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
    if (modelTransformationList is not null && !tilegrid)
    {

      var mt = ModelTransformation.FromIEnumerable(modelTransformationList);


      var corners = new List<List<double>>()
        {
            new() { 0, 0 },
            new() { 0, height },
            new() { width, 0 },
            new() { width, height }
        };

      var projected = corners.Select(corner => new List<double>()
        {
            mt.d + (mt.a * corner[0]) + (mt.b * corner[1]),
            mt.h + (mt.e * corner[0]) + (mt.f * corner[1]),
        });

      var xs = projected.Select((pt) => pt[0]);
      var ys = projected.Select((pt) => pt[1]);


      return new BoundingBox()
      {
        XMin = xs.Min(),
        YMin = ys.Min(),
        XMax = xs.Max(),
        YMax = ys.Max()
      };
    }
    else
    {
      var origin = this.GetOrigin();
      var resolution = this.GetResolution();

      var x1 = origin.X;
      var y1 = origin.Y;

      var x2 = x1 + (resolution.Item1 * width);
      var y2 = y1 + (resolution.Item2 * height);

      return new BoundingBox()
      {
        XMin = Math.Min(x1, x2),
        YMin = Math.Min(y1, y2),
        XMax = Math.Max(x1, x2),
        YMax = Math.Max(y1, y2),
      };
    }
  }

  /**
* Returns the number of samples per pixel.
* @returns {Number} the number of samples per pixel
*/
  private UInt64 GetSamplesPerPixel()
  {
    var samplesPerPixel = this.fileDirectory.GetFileDirectoryValue<UInt64>(FieldTypes.SamplesPerPixel);
    return samplesPerPixel != 0 ? samplesPerPixel : 1;
  }


  public uint GetBitsPerSample(int sampleIndex = 0)
  {
    var bitsPerSample = this.fileDirectory.GetFileDirectoryListValue<ushort>(FieldTypes.BitsPerSample).ToArray();
    return bitsPerSample[sampleIndex];
  }



  /// <summary>
  /// Returns the number of bytes per pixel.
  /// </summary>
  public int GetBytesPerPixel()
  {
    var bitsPerSample = this.fileDirectory.GetFileDirectoryListValue<ushort>(FieldTypes.BitsPerSample).ToArray();
    int bytes = 0;
    for (int i = 0; i < bitsPerSample.Length; ++i)
    {
      bytes += GetSampleByteSize(i);
    }
    return bytes;
  }

  /// <summary>
  /// Returns the byte size of a sample at the given index.
  /// </summary>
  public int GetSampleByteSize(int i)
  {
    var bitsPerSample = this.fileDirectory.GetFileDirectoryListValue<ushort>(FieldTypes.BitsPerSample).ToArray();
    if (i >= bitsPerSample.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(i), $"Sample index {i} is out of range.");
    }
    return (int)Math.Ceiling(bitsPerSample[i] / 8.0);
  }


  /// <summary>
  /// Returns the width of each tile.
  /// </summary>
  /// <returns>The width of each tile</returns>
  public uint GetTileWidth()
  {
    return this.isTiled
        ? this.fileDirectory.GetFileDirectoryValue<uint>(FieldTypes.TileWidth)
        : this.GetWidth();
  }

  /// <summary>
  /// Returns the height of each tile.
  /// </summary>
  /// <returns>The height of each tile</returns>
  public uint GetTileHeight()
  {
    if (this.isTiled)
    {
      return this.fileDirectory.GetFileDirectoryValue<uint>(FieldTypes.TileLength);
    }
    var rowsPerStrip = this.fileDirectory.GetFileDirectoryValue<uint?>(FieldTypes.RowsPerStrip);
    if (rowsPerStrip.HasValue)
    {
      return Math.Min(rowsPerStrip.Value, this.GetHeight());
    }
    return this.GetHeight();
  }


  /// <summary>
  /// TODO: Check type here, could be ushort
  /// </summary>
  /// <returns></returns>
  private int GetSampleFormat(int sampleIndex = 0)
  {
    var samplesFormat = this.fileDirectory.GetFileDirectoryListValue<int>(FieldTypes.SampleFormat);
    return samplesFormat?.ElementAt(sampleIndex) ?? 1;
  }

  private Array ArrayForType(int format, ulong bitsPerSample, ulong size)
  {
    switch (format)
    {
      case 1: // unsigned integer data
        if (bitsPerSample <= 8)
        {
          return new byte[size];
        }
        else if (bitsPerSample <= 16)
        {
          return new short[size];
        }
        else if (bitsPerSample <= 32)
        {
          return new UInt32[size];
        }
        break;
      case 2: // twos complement signed integer data
        switch (bitsPerSample)
        {
          case 8:
            return new sbyte[size];
          case 16:
            return new Int16[size];
          case 32:
            return new Int32[size];
        }

        break;
      case 3: // floating point data
        switch (bitsPerSample)
        {
          case 16:
          case 32:
            return new float[size];
          case 64:
            return new double[size];
        }
        break;
      default:
        break;
    }
    throw new Exception("Unsupported data format/bitsPerSample");
  }
  
  /// <summary>
  /// TODO: this is inefficient as a copy happens, which doens't happen in JS. It only creates a typed view over the data.
  /// </summary>
  /// <param name="format"></param>
  /// <param name="bitsPerSample"></param>
  /// <param name="buffer"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  private Array ArrayForType(int format, ulong bitsPerSample, ArrayBuffer buffer)
  {
    switch (format)
    {
      case 1: // unsigned integer data
        if (bitsPerSample <= 8)
        {
          return new byte[buffer.Length];
        }
        else if (bitsPerSample <= 16)
        {
          return new short[buffer.Length];
        }
        else if (bitsPerSample <= 32)
        {
          return new UInt32[buffer.Length];
        }
        break;
      case 2: // twos complement signed integer data
        switch (bitsPerSample)
        {
          case 8:
            return new sbyte[buffer.Length];
          case 16:
            return new Int16[buffer.Length];
          case 32:
            return new Int32[buffer.Length];
        }

        break;
      case 3: // floating point data
        switch (bitsPerSample)
        {
          case 16:
          case 32:
            return new float[buffer.Length];
          case 64:
            return new double[buffer.Length];
        }
        break;
      default:
        break;
    }
    throw new Exception("Unsupported data format/bitsPerSample");
  }
  
  
  /// <summary>
  /// TODO: this is inefficient as a copy happens, which doens't happen in JS. It only creates a typed view over the data.
  /// </summary>
  /// <param name="format"></param>
  /// <param name="bitsPerSample"></param>
  /// <param name="buffer"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  private DataView ArrayForType(int format, ulong bitsPerSample, int size)
  {
    switch (format)
    {
      case 1: // unsigned integer data
        if (bitsPerSample <= 8)
        {
          return new DataView(size, DataView.UINT8);
        }
        else if (bitsPerSample <= 16)
        {
          throw new NotImplementedException();
          // return new short[size];
        }
        else if (bitsPerSample <= 32)
        {
          return new DataView(size, DataView.UINT32);
        }
        break;
      case 2: // twos complement signed integer data
        switch (bitsPerSample)
        {
          case 8:
            throw new NotImplementedException();
            // return new sbyte[size];
          case 16:
            throw new NotImplementedException();
            // return new Int16[size];
          case 32:
            return new DataView(size, DataView.INT32);
        }

        break;
      case 3: // floating point data
        switch (bitsPerSample)
        {
          case 16:
          case 32:
            return new DataView(size, DataView.FLOAT);
          case 64:
            return new DataView(size, DataView.DOUBLE);
        }
        break;
      default:
        break;
    }
    throw new Exception("Unsupported data format/bitsPerSample");
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="sampleIndex"></param>
  /// <param name="size">ulong type here is confirmed by geotiff spec</param>
  /// <returns></returns>
  private Array GetArrayForSample(int sampleIndex, ulong size)
  {
    var format = this.GetSampleFormat(sampleIndex);
    var bitsPerSample = this.GetBitsPerSample(sampleIndex);
    return this.ArrayForType(format, bitsPerSample, size);
  }
  
  private Array GetArrayForSample(int sampleIndex, ArrayBuffer buffer)
  {
    var format = this.GetSampleFormat(sampleIndex);
    var bitsPerSample = this.GetBitsPerSample(sampleIndex);
    return this.ArrayForType(format, bitsPerSample, buffer);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  public async Task<List<Array>> ReadRasters(ImageWindow? window = null, CancellationToken? cancellationToken = null)
  {
    var imageWindow = new uint[] { 0, 0, this.GetWidth(), this.GetHeight() };

    if (window is not null)
    {
      imageWindow[0] = window.left;
      imageWindow[1] = window.top ;
      imageWindow[2] = window.right;
      imageWindow[3] = window.bottom;
    }
    
    if (imageWindow[0] > imageWindow[2] || imageWindow[1] > imageWindow[3])
    {
      throw new Exception("Invalid subsets");
    }

    var imageWindowWidth = imageWindow[2] - imageWindow[0];
    var imageWindowHeight = imageWindow[3] - imageWindow[1];

    var numPixels = (ulong)imageWindowWidth * (ulong)imageWindowHeight;// ignore resharper telling you that cast is redundant.
    var samplesPerPixel = this.GetSamplesPerPixel();
    var samples = Enumerable.Range(0, (int)samplesPerPixel).ToArray(); // TODO: change away from using Enumerable.Range here as it doesn't accept ulong, or write extension.
    List<Array> valueArrays = new();
    for (var i = 0; i < samples.Count(); ++i)
    {
      var valueArray = this.GetArrayForSample(samples[i], numPixels);
      valueArrays.Add(valueArray);
    }

    var poolOrDecoder = new DecoderRegistry();
    return await this._ReadRaster(imageWindow, samples, valueArrays, false, poolOrDecoder, null, null, cancellationToken);
    
  }


  private Func<DataView, int, object> GetReaderForSample(int sampleIndex)
  {
    // JSENH - potentially refactor this in js
    int format = GetSampleFormat(sampleIndex);


    uint bitsPerSample = GetBitsPerSample(sampleIndex); ;

    switch (format)
    {
      case 1: // unsigned integer data
        if (bitsPerSample <= 8)
          return (dv, offset) => dv.getUint8(offset);
        else if (bitsPerSample <= 16)
          return (dv, offset) => dv.getUint16(offset);
        else if (bitsPerSample <= 32)
          return (dv, offset) => dv.getUint32(offset);
        break;

      case 2: // signed integer data
        throw new NotImplementedException("Signed integers not supported");
      //       if (bitsPerSample <= 8)
      //   return (dv, offset) => dv.GetInt8(offset);
      // else if (bitsPerSample <= 16)
      //   return (dv, offset) => dv.GetInt16(offset);
      // else if (bitsPerSample <= 32)
      //   return (dv, offset) => dv.GetInt32(offset);
      //       break;

      case 3: // floating point data
        switch (bitsPerSample)
        {
          case 16:
            throw new NotImplementedException("Float 16 not implemented");
          // return (dv, offset) => GetFloat16(dv, offset); // You need to implement GetFloat16
          case 32:
            return (dv, offset) => dv.getFloat32(offset);
          case 64:
            return (dv, offset) => dv.getFloat64(offset);
        }
        break;
    }

    throw new Exception("Unsupported data format/bitsPerSample");
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="imageWindow"></param>
  /// <param name="samples"></param>
  /// <param name="valueArrays"></param>
  /// <param name="interleave"></param>
  /// <param name="decoder"></param>
  /// <param name="width">TODO: consider that this can be null</param>
  /// <param name="height">TODO: consider that this can be null</param>
  /// <param name="cancellationToken"></param>
  /// <exception cref="NotImplementedException"></exception>
  private async Task<List<Array>> _ReadRaster(uint[] imageWindow, int[] samples, List<Array> valueArrays, bool interleave, DecoderRegistry decoder, uint? width, uint? height, CancellationToken? cancellationToken)
  {
    var tileWidth = this.GetTileWidth();
    var tileHeight = this.GetTileHeight();
    var imageWidth = this.GetWidth();
    var imageHeight = this.GetHeight();
    var minXTile = (int)Math.Max(Math.Floor((double)imageWindow[0] / (double)tileWidth), 0);
    var maxXTile = Math.Min(
      Math.Ceiling((double)imageWindow[2] / (double)tileWidth),
      Math.Ceiling((double)imageWidth / tileWidth)
    );
    var minYTile = (int)Math.Max(Math.Floor((double)imageWindow[1] / (double)tileHeight), 0);
    var maxYTile = (int)Math.Min(
      Math.Ceiling((double)imageWindow[3] / (double)tileHeight),
      Math.Ceiling((double)imageHeight / (double)tileHeight)
    );
    var windowWidth = imageWindow[2] - imageWindow[0];

    var bytesPerPixel = this.GetBytesPerPixel();
    
    List<int> srcSampleOffsets = new();
    List<Func<DataView,long,bool,object>> sampleReaders = new();
    for (var i = 0; i < samples.Length; ++i)
    {
      if (this.planarConfiguration == 1)
      {
        srcSampleOffsets.Add(sum(this.fileDirectory.BitsPerSample, 0, samples[i]) / 8);
      }
      else
      {
        srcSampleOffsets.Add(0);
      }
      sampleReaders.Add(this.getReaderForSample(samples[i]));
    }

    var promises = new List<Task>();


    for (var yTile = minYTile; yTile < maxYTile; ++yTile)
    {
      for (var xTile = minXTile; xTile < maxXTile; ++xTile)
      {
        Task<TileOrStripResult> getPromise = null;
        if (this.planarConfiguration == 1)
        {
          getPromise = this.getTileOrStrip(xTile, yTile, 0, new DecoderRegistry(), cancellationToken);
        }
        for (var sampleIndex = 0; sampleIndex < samples.Length; ++sampleIndex)
        {
          var si = sampleIndex;
          var sample = samples[sampleIndex];
          if (this.planarConfiguration == 2)
          {
            bytesPerPixel = this.GetSampleByteSize(sample);
            getPromise = this.getTileOrStrip(xTile, yTile, sample, new DecoderRegistry(), cancellationToken);
          }

          var promise = getPromise.Then<TileOrStripResult,bool>((tile) => 
          {
            
            var buffer = tile.data;
            var dataView = new DataView(buffer);
            var blockHeight = this.GetBlockHeight(tile.y);
            var firstLine = tile.y * tileHeight;
            var firstCol = tile.x * tileWidth;
            var lastLine = firstLine + blockHeight;
            var lastCol = (tile.x + 1) * tileWidth;
            var reader = sampleReaders[si];

            var ymax = JSMath.Min(blockHeight, blockHeight - (lastLine - imageWindow[3]), imageHeight - firstLine);
            var xmax = JSMath.Min((ulong)tileWidth, (ulong)(tileWidth - (lastCol - imageWindow[2])), (ulong)(imageWidth - firstCol));

            for (var y = Math.Max(0, imageWindow[1] - firstLine); y < ymax; ++y)
            {
              for (var x = Math.Max(0, imageWindow[0] - firstCol); (ulong)x < xmax; ++x)
              {
                var pixelOffset = ((y * tileWidth) + x) * bytesPerPixel;
                var value = reader(
                  dataView, (pixelOffset + srcSampleOffsets[si]), littleEndian
                );
                long windowCoordinate;
                if (interleave)
                {
                  // JSENH looks like array is being set to object?
                  throw new NotImplementedException("Something weird with geotiff.js here so not supported yet");
                  // windowCoordinate = ((y + firstLine - imageWindow[1]) * windowWidth * samples.Length)
                  //   + ((x + firstCol - imageWindow[0]) * samples.Length)
                  //   + si;
                  // valueArrays[(int)windowCoordinate] = value;
                }
                else
                {
                  windowCoordinate = (
                    (y + firstLine - imageWindow[1]) * windowWidth
                  ) + x + firstCol - imageWindow[0];
                  var myArray = valueArrays[si];
                  myArray.SetValue(value, (int)windowCoordinate);
                  // valueArrays[si][(int)windowCoordinate] = value;
                }
              }
            }

            return true;
          });
          promises.Add(promise);
        }
      }
    }

    await Task.WhenAll(promises);
    

    if ((width != null && (imageWindow[2] - imageWindow[0]) != width)
        || (height != null && (imageWindow[3] - imageWindow[1]) != height))
    {
      throw new NotImplementedException("Resampling not yet implmented");
      // var resampled;
      // if (interleave)
      // {
      //   resampled = resampleInterleaved(
      //     valueArrays,
      //     imageWindow[2] - imageWindow[0],
      //     imageWindow[3] - imageWindow[1],
      //     width, height,
      //     samples.length,
      //     resampleMethod,
      //   );
      // }
      // else
      // {
      //   resampled = resample(
      //     valueArrays,
      //     imageWindow[2] - imageWindow[0],
      //     imageWindow[3] - imageWindow[1],
      //     width, height,
      //     resampleMethod,
      //   );
      // }
      // resampled.width = width;
      // resampled.height = height;
      // return resampled;
    }

    // valueArrays.width = width || imageWindow[2] - imageWindow[0];
    // valueArrays.height = height || imageWindow[3] - imageWindow[1];

    return valueArrays;
  }

  
  /** @typedef {import("./geotiff.js").TypedArray} TypedArray */
  /** @typedef {import("./geotiff.js").ReadRasterResult} ReadRasterResult */

  private T sum<T>(IEnumerable<T> array, int start, int end) where T : INumber<T> {
    T s = T.AdditiveIdentity;
    for (var i = start; i < end; ++i) {
      s += array.ElementAt(i);
    }
    return s;
  }
  
  
  public Func<DataView, long, bool, object> getReaderForSample(int sampleIndex)
  {
    var format = this.fileDirectory.SampleFormat is not null
      ? this.fileDirectory.SampleFormat[sampleIndex] : 1;
    var bitsPerSample = this.fileDirectory.BitsPerSample[sampleIndex];
    switch (format)
    {
      case 1: // unsigned integer data
        if (bitsPerSample <= 8)
        {
          return (dv, offset, endianNess) => dv.getUint8((int)offset);
          // return (dv, offset) => dv.getUint8((int)offset);
        }
        else if (bitsPerSample <= 16)
        {
          return (dv, offset, endianNess) => dv.getUint16((int)offset, endianNess);
        }
        else if (bitsPerSample <= 32)
        {
          return (dv, offset, endianNess) => dv.getUint32((int)offset, endianNess);
        }
        break;
      case 2: // twos complement signed integer data
        if (bitsPerSample <= 8)
        {
          return (dv, offset, endianNess) => dv.getInt8((int)offset);
        }
        else if (bitsPerSample <= 16)
        {
          return (dv, offset, endianNess) => dv.getUint16((int)offset, endianNess);
        }
        else if (bitsPerSample <= 32)
        {
          return (dv, offset, endianNess) => dv.getInt32((int)offset, endianNess);
        }
        break;
      case 3:
        switch (bitsPerSample)
        {
          case 16:
            throw new NotImplementedException();
          case 32:
            return (dv, offset, endianNess) => dv.getFloat32((int)offset, endianNess);
          case 64:
            return (dv, offset, endianNess) => dv.getFloat64((int)offset, endianNess);
          default:
            break;
        }
        break;
      default:
        break;
    }
    throw new Exception("Unsupported data format/bitsPerSample");
  }

  /**
 * Returns the decoded strip or tile.
 * @param {Number} x the strip or tile x-offset
 * @param {Number} y the tile y-offset (0 for stripped images)
 * @param {Number} sample the sample to get for separated samples
 * @param {import("./geotiff").Pool|import("./geotiff").BaseDecoder} poolOrDecoder the decoder or decoder pool
 * @param {AbortSignal} [signal] An AbortSignal that may be signalled if the request is
 *                               to be aborted
 * @returns {Promise.<{x: number, y: number, sample: number, data: ArrayBuffer}>} the decoded strip or tile
 */
  async Task<TileOrStripResult> getTileOrStrip(int x, int y, int sample, DecoderRegistry poolOrDecoder, CancellationToken? signal)
  {
    var numTilesPerRow = (int) Math.Ceiling((int)this.GetWidth() / (double)this.GetTileWidth());
    var numTilesPerCol = (int)Math.Ceiling((double)this.GetHeight() / (double)this.GetTileHeight());
    var index = 0;

    if (this.planarConfiguration == 1)
    {
      index = (y * numTilesPerRow) + x;
    }
    else if (this.planarConfiguration == 2)
    {
      index = (sample * numTilesPerRow * numTilesPerCol) + (y * numTilesPerRow) + x;
    }

    int offset;
    int byteCount;
    if (this.isTiled)
    {
      offset = this.fileDirectory.GetFileDirectoryListValue<int>("TileOffsets").ElementAt(index);
      byteCount = this.fileDirectory.GetFileDirectoryListValue<int>("TileByteCounts").ElementAt(index);
    }
    else
    {
      offset = this.fileDirectory.GetFileDirectoryListValue<int>("StripOffsets").ElementAt(index);
      byteCount = this.fileDirectory.GetFileDirectoryListValue<int>("StripByteCounts").ElementAt(index);
    }

    if (byteCount == 0)
    {
      var nPixels = this.GetBlockHeight(y) * this.GetTileWidth();
      var bytesPerPixel = (this.planarConfiguration == 2) ? this.GetSampleByteSize(sample) : this.GetBytesPerPixel();
      var data = new ArrayBuffer(nPixels * bytesPerPixel);
      var view = this.GetArrayForSample(sample, data);

      int valueToFill = 0;
      int? temp = this.GetGDALNoData();
      if (temp is not null)
      {
        valueToFill = (int)temp;
      }

      // TODO: Note that this will not actually set the values of the underlying ArrayBuffer.
      for (int i = 0; i < view.Length; i++)
      {
        view.SetValue(valueToFill,i); 
      }

      return new TileOrStripResult
      {
        x = x,
        y = y,
        data = data,
        sample = sample
      };
    }

    var slice = (await this.source.Fetch(new List<Slice>() { new Slice(offset, length: byteCount) }, signal)).First();

    Func<Task<ArrayBuffer>> request;
    ArrayBuffer finalData;
    if (tiles == null || tiles.ContainsKey(index) is false)
    {
      // resolve each request by potentially applying array normalization
      request = async () =>
      {
          // TODO: need to actually decode
        var data = await poolOrDecoder.Decode(this.fileDirectory, slice);
        // var data = slice;
        var sampleFormat = this.GetSampleFormat();
        var bitsPerSample = this.GetBitsPerSample();
        if (needsNormalization(sampleFormat, (int)bitsPerSample))
        {
          data = normalizeArray(
            data,
            sampleFormat,
            this.planarConfiguration,
            (int)this.GetSamplesPerPixel(),
            (int)bitsPerSample,
            (int)this.GetTileWidth(),
            (int)this.GetBlockHeight(y)
          );
        }
        return data;
      };
      finalData= await request();
      // set the cache
      if (tiles != null)
      {
        tiles[index] = finalData;
      }
    }
    else
    {
      // get from the cache
      finalData = tiles[index];
    }

    // cache the tile request
    return new TileOrStripResult() { x=x, y=y, sample=sample, data = finalData };
  }
  
  private bool needsNormalization(int format, int bitsPerSample) {
    if ((format == 1 || format == 2) && bitsPerSample <= 32 && bitsPerSample % 8 == 0) {
      return false;
    } else if (format == 3 && (bitsPerSample == 16 || bitsPerSample == 32 || bitsPerSample == 64)) {
      return false;
    }
    return true;
  }
  
  public long getBlockWidth() {
    return this.GetTileWidth();
  }

  public long GetBlockHeight(int y) {
    if (this.isTiled || (y + 1) * this.GetTileHeight() <= this.GetHeight()) {
      return this.GetTileHeight();
    } else {
      return this.GetHeight() - (y * this.GetTileHeight());
    }
  }
  
  /**
   * Returns the GDAL nodata value
   * @returns {number|null}
   */
  private int? GetGDALNoData()
  {
    if (this.fileDirectory.GDAL_NODATA == null)
    {
      return null;
    }
    var str = this.fileDirectory.GDAL_NODATA;
    return int.Parse(str.Substring(0, str.Length - 1));
  }


  private ArrayBuffer normalizeArray(ArrayBuffer inBuffer, int format, int planarConfiguration, int samplesPerPixel, int bitsPerSample, int tileWidth, int tileHeight)
  {
    // var inByteArray = new Uint8Array(inBuffer);
    var view = new DataView(inBuffer); //JSENH Check this
    var outSize = planarConfiguration == 2
      ? tileHeight * tileWidth
      : tileHeight * tileWidth * samplesPerPixel;
    var samplesToTransfer = planarConfiguration == 2
      ? 1 : samplesPerPixel;
    
    var outArray = ArrayForType(format, (ulong)bitsPerSample, outSize);
    // var pixel = 0;
    var bitMask = JsParse.ParseInt(new String('1', bitsPerSample),2);
    
    if (format == 1)
    { // unsigned integer
      // translation of https://github.com/OSGeo/gdal/blob/master/gdal/frmts/gtiff/geotiff.cpp#L7337
      int pixelBitSkip;
      // var sampleBitOffset = 0;
      if (planarConfiguration == 1)
      {
        pixelBitSkip = samplesPerPixel * bitsPerSample;
        // sampleBitOffset = (samplesPerPixel - 1) * bitsPerSample;
      }
      else
      {
        pixelBitSkip = bitsPerSample;
      }

      // Bits per line rounds up to next byte boundary.
      var bitsPerLine = tileWidth * pixelBitSkip;
      if ((bitsPerLine & 7) != 0)
      {
        bitsPerLine = (bitsPerLine + 7) & (~7);
      }

      for (var y = 0; y < tileHeight; ++y)
      {
        var lineBitOffset = y * bitsPerLine;
        for (var x = 0; x < tileWidth; ++x)
        {
          var pixelBitOffset = lineBitOffset + (x * samplesToTransfer * bitsPerSample);
          for (var i = 0; i < samplesToTransfer; ++i)
          {
            var bitOffset = pixelBitOffset + (i * bitsPerSample);
            var outIndex = (((y * tileWidth) + x) * samplesToTransfer) + i;
            
            var byteOffset = (int)Math.Floor(bitOffset / 8.0);
            var innerBitOffset = bitOffset % 8;
            if (innerBitOffset + bitsPerSample <= 8)
            {
              var result = (view.getUint8(byteOffset) >> (8 - bitsPerSample) - innerBitOffset) & bitMask;
              outArray.SetValue(result, outIndex); // TODO: not sure why they don't care about endianness here.
            }
            else if (innerBitOffset + bitsPerSample <= 16)
            {
              var result = (view.getUint16(byteOffset) >> (16 - bitsPerSample) - innerBitOffset) & bitMask;
              outArray.SetValue(result, outIndex);// TODO: not sure why they don't care about endianness here.
            }
            else if (innerBitOffset + bitsPerSample <= 24)
            {
              var raw = (view.getUint16(byteOffset) << 8) | (view.getUint8(byteOffset + 2));
              var result = (raw >> (24 - bitsPerSample) - innerBitOffset) & bitMask;
              outArray.SetValue(result, outIndex);// TODO: not sure why they don't care about endianness here.
            }
            else
            {
              var result =(view.getUint32(byteOffset) >> (32 - bitsPerSample) - innerBitOffset) & bitMask;
              outArray.SetValue((int)result, outIndex);// TODO: fix narrowing conversion // TODO: not sure why they don't care about endianness here.
            }
// THIS IS COMMENTED OUT IN GEOTIFF.JS
            // var outWord = 0;
            // for (var bit = 0; bit < bitsPerSample; ++bit) {
            //   if (inByteArray[bitOffset >> 3]
            //     & (0x80 >> (bitOffset & 7))) {
            //     outWord |= (1 << (bitsPerSample - 1 - bit));
            //   }
            //   ++bitOffset;
            // }

            // outArray[outIndex] = outWord;
            // outArray[pixel] = outWord;
            // pixel += 1;
          }
          // bitOffset = bitOffset + pixelBitSkip - bitsPerSample;
// THIS IS COMMENTED OUT IN GEOTIFF.JS
        }
      }
    }
    else if (format == 3)
    { 
// THIS IS COMMENTED OUT IN GEOTIFF.JS
      // floating point
      // Float16 is handled elsewhere
      // normalize 16/24 bit floats to 32 bit floats in the array
      // console.time();
      // if (bitsPerSample == 16) {
      //   for (var byte = 0, outIndex = 0; byte < inBuffer.byteLength; byte += 2, ++outIndex) {
      //     outArray[outIndex] = getFloat16(view, byte);
      //   }
      // }
      // console.timeEnd()
// THIS IS COMMENTED OUT IN GEOTIFF.JS      
    }

    return outArray.ToArrayBuffer();
  }
  
  
  /// <summary>
  /// Not part of GeoTiff.js
  /// </summary>
  /// <returns></returns>
  public int? GetProjectionString()
  {
    return this.fileDirectory.GetGeoDirectoryValue<int?>("GeographicTypeGeoKey");
  }

  /// <summary>
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task<List<Array>> ReadValueAtCoordinate(double x, double y, CancellationToken? cancellationToken = null)
  {
    var modelTransformationList = this.fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
    if (modelTransformationList is not null)
    {
      throw new NotImplementedException("Model transformations not yet supported");
    }
    
    //TODO: Check not out of bounds
    var origin = this.GetOrigin();
    var res = this.GetResolution();
    // If the user passed a low x, we want to be close to the orgin.
    var left = (x - origin.X) / res.Item1;
    var right = left + res.Item1;

    // if the user passed a low y, be far away from the origin.

    var top = (y - origin.Y) / res.Item2;
    var bottom = top + 1;

    var window = new ImageWindow()
    {
      left = (uint)left,
      right = (uint)right,
      bottom = (uint)bottom,
      top = (uint)top
    };
    
    return await this.ReadRasters(window, cancellationToken);
  }
}


public class TileOrStripResult
{
  public int x { get; set; }
  public int y { get; set; }
  public int sample { get; set; }
  public Task request { get; set; }
  public ArrayBuffer data { get; set; }

}