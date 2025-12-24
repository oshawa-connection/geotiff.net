using Geotiff.Compression;
using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public class GeoTiffImage
{
    public readonly ImageFileDirectory fileDirectory;
    public readonly bool littleEndian;
    private readonly bool cache;
    private readonly BaseSource source;
    private readonly Dictionary<int, ArrayBuffer>? tiles;
    private readonly bool isTiled;
    private readonly ushort planarConfiguration;

    public GeoTiffImage(ImageFileDirectory fileDirectory, bool littleEndian, bool cache, BaseSource source)
    {
        this.fileDirectory = fileDirectory;
        this.littleEndian = littleEndian;
        tiles = cache ? new Dictionary<int, ArrayBuffer>() : null;

        isTiled = fileDirectory.FileDirectory.ContainsKey("StripOffsets") is false;
        ushort? planarConfiguration = fileDirectory.GetFileDirectoryValue<ushort?>(FieldTypes.PlanarConfiguration);

        if (planarConfiguration is null)
        {
            this.planarConfiguration = 1;
        }
        else
        {
            this.planarConfiguration = (ushort)planarConfiguration;
        }

        if (this.planarConfiguration != 1 && this.planarConfiguration != 2)
        {
            throw new InvalidTiffException("Invalid planar configuration.");
        }

        this.source = source;
    }

    public bool HasValidTiePoints()
    {
        IEnumerable<double>? tiePoint = fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTiepoint);
        return tiePoint is not null && tiePoint.Count() == 6;
    }

    public bool HasValidModelTransformation()
    {
        IEnumerable<double>? modelTransformation =
            fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);

        return modelTransformation is not null;
    }

    /// <summary>
    /// Can use this before checking for origin or boundingbox to prevent exceptions
    /// </summary>
    /// <returns></returns>
    public bool HasAffineTransformation()
    {
        return HasValidModelTransformation() || HasValidTiePoints();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException">Thrown if the affine transformation is not set</exception>
    public VectorXYZ? GetOrigin()
    {
        IEnumerable<double>? tiePoint = fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTiepoint);
        IEnumerable<double>? modelTransformation =
            fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);

        if (HasValidTiePoints())
        {
            return new VectorXYZ() { X = tiePoint.ElementAt(3), Y = tiePoint.ElementAt(4), Z = tiePoint.ElementAt(5) };
        }

        if (modelTransformation is not null)
        {
            // TODO: check modeltransformation length

            return new VectorXYZ()
            {
                X = modelTransformation.ElementAt(3),
                Y = modelTransformation.ElementAt(7),
                Z = modelTransformation.ElementAt(11)
            };
        }

        throw new GeoTiffException("The image does not have an affine transformation.");
    }


    /// <summary>
    /// uint return type here is confirmed by geotiff spec
    /// </summary>
    /// <returns></returns>
    public uint GetWidth()
    {
        return fileDirectory.GetFileDirectoryValue<uint>(FieldTypes.ImageWidth);
    }

    /// <summary>
    /// uint return type here is confirmed by geotiff spec
    /// </summary>
    /// <returns></returns>
    public uint GetHeight()
    {
        return fileDirectory.GetFileDirectoryValue<uint>(FieldTypes.ImageLength);
    }

    public Tuple<double, double, double> GetResolution()
    {
        IEnumerable<double>? modelPixelScaleR =
            fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelPixelScale);
        IEnumerable<double>? modelTransformationR =
            fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);

        if (modelPixelScaleR is not null)
        {
            double[] modelPixelScale = modelPixelScaleR.ToArray();
            return new Tuple<double, double, double>(
                modelPixelScale[0],
                -modelPixelScale[1],
                modelPixelScale[2]);
        }

        if (modelTransformationR is not null)
        {
            double[] modelTransformation = modelTransformationR.ToArray();
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

        throw new GeoTiffException("The image does not have an affine transformation.");
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
        uint height = GetHeight();
        uint width = GetWidth();
        IEnumerable<double>? modelTransformationList =
            fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
        if (modelTransformationList is not null && !tilegrid)
        {
            ModelTransformation mt = ModelTransformation.FromIEnumerable(modelTransformationList);


            var corners = new List<List<double>>()
            {
                new() { 0, 0 }, new() { 0, height }, new() { width, 0 }, new() { width, height }
            };

            IEnumerable<List<double>> projected = corners.Select(corner => new List<double>()
            {
                mt.d + (mt.a * corner[0]) + (mt.b * corner[1]), mt.h + (mt.e * corner[0]) + (mt.f * corner[1])
            });

            IEnumerable<double> xs = projected.Select((pt) => pt[0]);
            IEnumerable<double> ys = projected.Select((pt) => pt[1]);


            return new BoundingBox() { XMin = xs.Min(), YMin = ys.Min(), XMax = xs.Max(), YMax = ys.Max() };
        }
        else
        {
            VectorXYZ origin = GetOrigin();
            Tuple<double, double, double> resolution = GetResolution();

            double x1 = origin.X;
            double y1 = origin.Y;

            double x2 = x1 + (resolution.Item1 * width);
            double y2 = y1 + (resolution.Item2 * height);

            return new BoundingBox()
            {
                XMin = Math.Min(x1, x2), YMin = Math.Min(y1, y2), XMax = Math.Max(x1, x2), YMax = Math.Max(y1, y2)
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ulong GetSamplesPerPixel()
    {
        ulong samplesPerPixel = fileDirectory.GetFileDirectoryValue<ulong>(FieldTypes.SamplesPerPixel);
        return samplesPerPixel != 0 ? samplesPerPixel : 1;
    }


    public uint GetBitsPerSample(int sampleIndex = 0)
    {
        ushort[] bitsPerSample = fileDirectory.GetFileDirectoryListValue<ushort>(FieldTypes.BitsPerSample).ToArray();
        return bitsPerSample[sampleIndex];
    }


    /// <summary>
    /// Returns the number of bytes per pixel.
    /// </summary>
    public int GetBytesPerPixel()
    {
        ushort[] bitsPerSample = fileDirectory.GetFileDirectoryListValue<ushort>(FieldTypes.BitsPerSample).ToArray();
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
        ushort[] bitsPerSample = fileDirectory.GetFileDirectoryListValue<ushort>(FieldTypes.BitsPerSample).ToArray();
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
        return isTiled
            ? fileDirectory.GetFileDirectoryValue<uint>(FieldTypes.TileWidth)
            : GetWidth();
    }

    /// <summary>
    /// Returns the height of each tile.
    /// </summary>
    /// <returns>The height of each tile</returns>
    public uint GetTileHeight()
    {
        if (isTiled)
        {
            return fileDirectory.GetFileDirectoryValue<uint>(FieldTypes.TileLength);
        }

        uint? rowsPerStrip = fileDirectory.GetFileDirectoryValue<uint?>(FieldTypes.RowsPerStrip);
        if (rowsPerStrip.HasValue)
        {
            return Math.Min(rowsPerStrip.Value, GetHeight());
        }

        return GetHeight();
    }


    /// <summary>
    /// TODO: Check type here, could be ushort
    /// </summary>
    /// <returns></returns>
    private int GetSampleFormat(int sampleIndex = 0)
    {
        IEnumerable<int>? samplesFormat = fileDirectory.GetFileDirectoryListValue<int>(FieldTypes.SampleFormat);
        return samplesFormat?.ElementAt(sampleIndex) ?? 1;
    }

    private Array ArrayForType(int format, ulong bitsPerSample, ulong size1, ulong? size2 = null)
    {
        switch (format)
        {
            case 1: // unsigned integer data
                if (bitsPerSample <= 8)
                {
                    return new byte[size1];
                }
                else if (bitsPerSample <= 16)
                {
                    return new short[size1];
                }
                else if (bitsPerSample <= 32)
                {
                    return new uint[size1];
                }

                break;
            case 2: // twos complement signed integer data
                switch (bitsPerSample)
                {
                    case 8:
                        return new sbyte[size1];
                    case 16:
                        return new short[size1];
                    case 32:
                        return new int[size1];
                }

                break;
            case 3: // floating point data
                switch (bitsPerSample)
                {
                    case 16:
                    case 32:
                        return new float[size1];
                    case 64:
                        return new double[size1];
                }

                break;
            default:
                break;
        }

        throw new InvalidTiffException("Unsupported data format/bitsPerSample");
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
                    return new uint[buffer.Length];
                }

                break;
            case 2: // twos complement signed integer data
                switch (bitsPerSample)
                {
                    case 8:
                        return new sbyte[buffer.Length];
                    case 16:
                        return new short[buffer.Length];
                    case 32:
                        return new int[buffer.Length];
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

        throw new InvalidTiffException("Unsupported data format/bitsPerSample");
    }


    /// <summary>
    /// TODO: Check why there are two overloads for this method.
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
                    return new DataView(size, GeotiffSampleDataTypes.Uint8);
                }
                else if (bitsPerSample <= 16)
                {
                    throw new NotImplementedException();
                    // return new short[size];
                }
                else if (bitsPerSample <= 32)
                {
                    return new DataView(size, GeotiffSampleDataTypes.Uint32);
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
                        return new DataView(size, GeotiffSampleDataTypes.Int32);
                }

                break;
            case 3: // floating point data
                switch (bitsPerSample)
                {
                    case 16:
                    case 32:
                        return new DataView(size, GeotiffSampleDataTypes.Float32);
                    case 64:
                        return new DataView(size,GeotiffSampleDataTypes.Double);
                }

                break;
            default:
                break;
        }

        throw new InvalidTiffException("Unsupported data format/bitsPerSample");
    }

    /// <summary>
    /// Leave this an non-generic for now, allow app to throw invalid casts.
    /// </summary>
    /// <param name="sampleIndex"></param>
    /// <param name="size">ulong type here is confirmed by geotiff spec</param>
    /// <returns></returns>
    private Array GetArrayForSample(int sampleIndex, ulong size)
    {
        int format = GetSampleFormat(sampleIndex);
        uint bitsPerSample = GetBitsPerSample(sampleIndex);
        return ArrayForType(format, bitsPerSample, size);
    }

    private Array GetArrayForSample(int sampleIndex, ArrayBuffer buffer)
    {
        int format = GetSampleFormat(sampleIndex);
        uint bitsPerSample = GetBitsPerSample(sampleIndex);
        return ArrayForType(format, bitsPerSample, buffer);
    }

    /// <summary>
    /// By specifying the type parameter, you specify that all samples are of the same type. If this is not true, use GeoTIFFReadResultUnknownType instead.
    /// TODO: allow users to specify which samples they want to read rather than reading all of them.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<GeoTIFFReadResult<T>> ReadRastersAsync<T>(ImageWindow? window = null, CancellationToken? cancellationToken = null) where T : struct 
    {
        uint[] imageWindow = new uint[] { 0, 0, GetWidth(), GetHeight() };

        if (window is not null)
        {
            imageWindow[0] = window.Left;
            imageWindow[1] = window.Top;
            imageWindow[2] = window.Right;
            imageWindow[3] = window.Bottom;
        }

        if (imageWindow[0] > imageWindow[2] || imageWindow[1] > imageWindow[3])
        {
            throw new GeoTiffException("Invalid subsets");
        }

        uint imageWindowWidth = imageWindow[2] - imageWindow[0];
        uint imageWindowHeight = imageWindow[3] - imageWindow[1];

        ulong numPixels =
            (ulong)imageWindowWidth * (ulong)imageWindowHeight; // ignore resharper telling you that cast is redundant.
        // TODO: allow user to specify which samples to be read rather than reading all of them.
        ulong samplesPerPixel = GetSamplesPerPixel();
        int[] samples =
            Enumerable.Range(0, (int)samplesPerPixel)
                .ToArray(); // TODO: change away from using Enumerable.Range here as it doesn't accept ulong, or write extension.
        List<T[]> valueArrays = new();
        for (int i = 0; i < samples.Count(); ++i)
        {
            var valueArray = (T[])GetArrayForSample(samples[i], numPixels);
            valueArrays.Add(valueArray);
        }

        var poolOrDecoder = new DecoderRegistry();
        return await _ReadRasterAsync<T>(imageWindow, samples, valueArrays, poolOrDecoder, null, null, cancellationToken);
    }
    
    private int sum(IEnumerable<int> array, int start, int end) {
        var s = 0;
        for (var i = start; i < end; ++i) {
            s += array.ElementAt(i);
        }
        return s;
    }
    
    
    /// <summary>
    /// TODO: Check why deocderRegistry is not used here
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
    private async Task<GeoTIFFReadResult<T>> _ReadRasterAsync<T>(uint[] imageWindow, int[] samples, List<T[]> valueArrays,
        DecoderRegistry decoder, uint? width, uint? height, CancellationToken? cancellationToken) where T : struct
    {
        uint tileWidth = GetTileWidth();
        uint tileHeight = GetTileHeight();
        uint imageWidth = GetWidth();
        uint imageHeight = GetHeight();
        int minXTile = (int)Math.Max(Math.Floor((double)imageWindow[0] / (double)tileWidth), 0);
        double maxXTile = Math.Min(
            Math.Ceiling((double)imageWindow[2] / (double)tileWidth),
            Math.Ceiling((double)imageWidth / tileWidth)
        );
        int minYTile = (int)Math.Max(Math.Floor((double)imageWindow[1] / (double)tileHeight), 0);
        int maxYTile = (int)Math.Min(
            Math.Ceiling((double)imageWindow[3] / (double)tileHeight),
            Math.Ceiling((double)imageHeight / (double)tileHeight)
        );
        uint windowWidth = imageWindow[2] - imageWindow[0];

        int bytesPerPixel = GetBytesPerPixel();

        List<int> srcSampleOffsets = new();
        List<Func<DataView, long, bool, object>> sampleReaders = new();
        for (int i = 0; i < samples.Length; ++i)
        {
            if (planarConfiguration == 1)
            {
                // fileDirectory.BitsPerSample.ElementAt(samples[i])
                srcSampleOffsets.Add(sum(this.fileDirectory.BitsPerSample, 0, samples[i]) / 8);
                // srcSampleOffsets.Add(fileDirectory.BitsPerSample.Take(samples[i] / 8).Sum());
            }
            else
            {
                srcSampleOffsets.Add(0);
            }

            sampleReaders.Add(GetReaderForSample(samples[i]));
        }

        var promises = new List<Task>();


        for (int yTile = minYTile; yTile < maxYTile; ++yTile)
        {
            for (int xTile = minXTile; xTile < maxXTile; ++xTile)
            {
                Task<TileOrStripResult> getPromise = null;
                if (planarConfiguration == 1)
                {
                    getPromise = GetTileOrStripAsync(xTile, yTile, 0, new DecoderRegistry(), cancellationToken);
                }

                for (int sampleIndex = 0; sampleIndex < samples.Length; ++sampleIndex)
                {
                    int si = sampleIndex;
                    int sample = samples[sampleIndex];
                    if (planarConfiguration == 2)
                    {
                        bytesPerPixel = GetSampleByteSize(sample);
                        getPromise = GetTileOrStripAsync(xTile, yTile, sample, new DecoderRegistry(),
                            cancellationToken);
                    }

                    Task<bool> promise = getPromise.Then<TileOrStripResult, bool>((tile) =>
                    {
                        ArrayBuffer buffer = tile.data;
                        var dataView = new DataView(buffer);
                        long blockHeight = GetBlockHeight(tile.y);
                        long firstLine = tile.y * tileHeight;
                        long firstCol = tile.x * tileWidth;
                        long lastLine = firstLine + blockHeight;
                        long lastCol = (tile.x + 1) * tileWidth;
                        Func<DataView, long, bool, object>? reader = sampleReaders[si];

                        long ymax = JSMath.Min(blockHeight, blockHeight - (lastLine - imageWindow[3]),
                            imageHeight - firstLine);
                        ulong xmax = JSMath.Min((ulong)tileWidth, (ulong)(tileWidth - (lastCol - imageWindow[2])),
                            (ulong)(imageWidth - firstCol));

                        for (long y = Math.Max(0, imageWindow[1] - firstLine); y < ymax; ++y)
                        {
                            for (long x = Math.Max(0, imageWindow[0] - firstCol); (ulong)x < xmax; ++x)
                            {
                                long pixelOffset = ((y * tileWidth) + x) * bytesPerPixel;
                                object? value = reader(
                                    dataView, pixelOffset + srcSampleOffsets[si], littleEndian
                                );
                                long windowCoordinate;
                                
                                windowCoordinate = (
                                    (y + firstLine - imageWindow[1]) * windowWidth
                                ) + x + firstCol - imageWindow[0];
                                
                                Array? myArray = valueArrays[si];
                                myArray.SetValue(value, (int)windowCoordinate);
                                // valueArrays[si][(int)windowCoordinate] = value;
                            }
                        }

                        return true;
                    });
                    promises.Add(promise);
                }
            }
        }

        await Task.WhenAll(promises);
        
        if ((width != null && imageWindow[2] - imageWindow[0] != width)
            || (height != null && imageWindow[3] - imageWindow[1] != height))
        {
            throw new NotImplementedException("Resampling not yet implemented");
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
        // imageWidth
        //     imageHeight
        
        return new GeoTIFFReadResult<T>(valueArrays, imageWidth, imageHeight, this);
        // var finalResult = new List<Array[,]>();
        // foreach (var sample in valueArrays)
        // {
        //     // Use the correct element type for the 2D array
        //     var elementType = sample.GetType().GetElementType() ?? typeof(object);
        //     // var resizedSample = new Array[imageWidth,imageHeight];
        //     var resizedSample = (Array[,])Array.CreateInstance(elementType, imageWidth, imageHeight);
        //
        //     for (int i = 0; i < imageWidth; i++)
        //     {
        //         for (int j = 0; j < imageHeight; j++)
        //         {
        //             int flattenedIndex = i * (int)imageHeight + j;
        //             resizedSample.SetValue(sample.GetValue(flattenedIndex), i, j);
        //         }
        //     }
        //     finalResult.Add(resizedSample);
        // }
        //
        // return finalResult;
    }

    
    /// <summary>
    /// Technically TIFF does support different types for each sample, but almost no software/ tooling supports this.
    /// (including geotiff.NET for that matter). If your use case requires this please file an issue on GitHub.
    /// </summary>
    /// <param name="sampleIndex"></param>
    /// <returns></returns>
    public GeotiffSampleDataTypes GetSampleType(int sampleIndex = 0)
    {
        int format = fileDirectory.SampleFormat is not null
            ? fileDirectory.SampleFormat[sampleIndex]
            : 1;
        int bitsPerSample = fileDirectory.BitsPerSample[sampleIndex];
        switch (format)
        {
            case 1: // unsigned integer data
                if (bitsPerSample <= 8)
                {
                    return GeotiffSampleDataTypes.Uint8;
                }
                else if (bitsPerSample <= 16)
                {
                    return GeotiffSampleDataTypes.Uint16;
                }
                else if (bitsPerSample <= 32)
                {
                    return GeotiffSampleDataTypes.Uint32;
                }

                break;
            case 2: // twos complement signed integer data
                if (bitsPerSample <= 8)
                {
                    return GeotiffSampleDataTypes.Int8;
                }
                else if (bitsPerSample <= 16)
                {
                    return GeotiffSampleDataTypes.Int16;
                }
                else if (bitsPerSample <= 32)
                {
                    return GeotiffSampleDataTypes.Int32;
                }

                break;
            case 3:
                switch (bitsPerSample)
                {
                    case 16:
                        throw new NotImplementedException();
                    case 32:
                        return GeotiffSampleDataTypes.Float32;
                    case 64:
                        return GeotiffSampleDataTypes.Double;
                    default:
                        break;
                }

                break;
            default:
                break;
        }

        throw new InvalidTiffException("Unsupported data format/bitsPerSample");
    }
    
    
    private Func<DataView, long, bool, object> GetReaderForSample(int sampleIndex)
    {
        int format = fileDirectory.SampleFormat is not null
            ? fileDirectory.SampleFormat[sampleIndex]
            : 1;
        int bitsPerSample = fileDirectory.BitsPerSample[sampleIndex];
        switch (format)
        {
            case 1: // unsigned integer data
                if (bitsPerSample <= 8)
                {
                    return (dv, offset, endianNess) => dv.GetUint8((int)offset);
                }
                else if (bitsPerSample <= 16)
                {
                    return (dv, offset, endianNess) => dv.GetUint16((int)offset, endianNess);
                }
                else if (bitsPerSample <= 32)
                {
                    return (dv, offset, endianNess) => dv.GetUint32((int)offset, endianNess);
                }

                break;
            case 2: // twos complement signed integer data
                if (bitsPerSample <= 8)
                {
                    return (dv, offset, endianNess) => dv.GetInt8((int)offset);
                }
                else if (bitsPerSample <= 16)
                {
                    return (dv, offset, endianNess) => dv.GetInt16((int)offset, endianNess);
                }
                else if (bitsPerSample <= 32)
                {
                    return (dv, offset, endianNess) => dv.GetInt32((int)offset, endianNess);
                }

                break;
            case 3:
                switch (bitsPerSample)
                {
                    case 16:
                        throw new NotImplementedException();
                    case 32:
                        return (dv, offset, endianNess) => dv.GetFloat32((int)offset, endianNess);
                    case 64:
                        return (dv, offset, endianNess) => dv.GetFloat64((int)offset, endianNess);
                    default:
                        break;
                }

                break;
            default:
                break;
        }

        throw new InvalidTiffException("Unsupported data format/bitsPerSample");
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
    private async Task<TileOrStripResult> GetTileOrStripAsync(int x, int y, int sample, DecoderRegistry poolOrDecoder,
        CancellationToken? signal)
    {
        int numTilesPerRow = (int)Math.Ceiling((int)GetWidth() / (double)GetTileWidth());
        int numTilesPerCol = (int)Math.Ceiling((double)GetHeight() / (double)GetTileHeight());
        int index = 0;

        if (planarConfiguration == 1)
        {
            index = (y * numTilesPerRow) + x;
        }
        else if (planarConfiguration == 2)
        {
            index = (sample * numTilesPerRow * numTilesPerCol) + (y * numTilesPerRow) + x;
        }

        int offset;
        int byteCount;
        if (isTiled)
        {
            offset = fileDirectory.GetFileDirectoryListValue<int>("TileOffsets").ElementAt(index);
            byteCount = fileDirectory.GetFileDirectoryListValue<int>("TileByteCounts").ElementAt(index);
        }
        else
        {
            offset = fileDirectory.GetFileDirectoryListValue<int>("StripOffsets").ElementAt(index);
            byteCount = fileDirectory.GetFileDirectoryListValue<int>("StripByteCounts").ElementAt(index);
        }

        if (byteCount == 0)
        {
            long nPixels = GetBlockHeight(y) * GetTileWidth();
            int bytesPerPixel = planarConfiguration == 2
                ? GetSampleByteSize(sample)
                : GetBytesPerPixel();
            var data = new ArrayBuffer(nPixels * bytesPerPixel);
            Array view = GetArrayForSample(sample, data);

            int valueToFill = 0;
            int? temp = GetGDALNoData();
            if (temp is not null)
            {
                valueToFill = (int)temp;
            }

            // TODO: Note that this will not actually set the values of the underlying ArrayBuffer.
            for (int i = 0; i < view.Length; i++)
            {
                view.SetValue(valueToFill, i);
            }

            return new TileOrStripResult { x = x, y = y, data = data, sample = sample };
        }

        ArrayBuffer slice =
            (await source.FetchAsync(new List<Slice>() { new(offset, byteCount) }, signal)).First();

        Func<Task<ArrayBuffer>> request;
        ArrayBuffer finalData;
        if (tiles == null || tiles.ContainsKey(index) is false)
        {
            // resolve each request by potentially applying array normalization
            request = async () =>
            {
                ArrayBuffer data = await poolOrDecoder.DecodeAsync(fileDirectory, slice);
                // var data = slice;
                int sampleFormat = GetSampleFormat();
                uint bitsPerSample = GetBitsPerSample();
                if (NeedsNormalization(sampleFormat, (int)bitsPerSample))
                {
                    data = NormalizeArray(
                        data,
                        sampleFormat,
                        planarConfiguration,
                        (int)GetSamplesPerPixel(),
                        (int)bitsPerSample,
                        (int)GetTileWidth(),
                        (int)GetBlockHeight(y)
                    );
                }

                return data;
            };
            finalData = await request();
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
        return new TileOrStripResult() { x = x, y = y, sample = sample, data = finalData };
    }

    private bool NeedsNormalization(int format, int bitsPerSample)
    {
        if ((format == 1 || format == 2) && bitsPerSample <= 32 && bitsPerSample % 8 == 0)
        {
            return false;
        }
        else if (format == 3 && (bitsPerSample == 16 || bitsPerSample == 32 || bitsPerSample == 64))
        {
            return false;
        }

        return true;
    }

    public long GetBlockWidth()
    {
        return GetTileWidth();
    }

    public long GetBlockHeight(int y)
    {
        if (isTiled || (y + 1) * GetTileHeight() <= GetHeight())
        {
            return GetTileHeight();
        }
        else
        {
            return GetHeight() - (y * GetTileHeight());
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int? GetGDALNoData()
    {
        if (fileDirectory.GDAL_NODATA == null)
        {
            return null;
        }

        string? str = fileDirectory.GDAL_NODATA;
        return int.Parse(str.Substring(0, str.Length - 1));
    }


    private ArrayBuffer NormalizeArray(ArrayBuffer inBuffer, int format, int planarConfiguration, int samplesPerPixel,
        int bitsPerSample, int tileWidth, int tileHeight)
    {
        // var inByteArray = new Uint8Array(inBuffer);
        var view = new DataView(inBuffer); //JSENH Check this
        int outSize = planarConfiguration == 2
            ? tileHeight * tileWidth
            : tileHeight * tileWidth * samplesPerPixel;
        int samplesToTransfer = planarConfiguration == 2
            ? 1
            : samplesPerPixel;

        DataView outArray = ArrayForType(format, (ulong)bitsPerSample, outSize);
        // var pixel = 0;
        int bitMask = JsParse.ParseInt(new string('1', bitsPerSample), 2);

        if (format == 1)
        {
            // unsigned integer
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
            int bitsPerLine = tileWidth * pixelBitSkip;
            if ((bitsPerLine & 7) != 0)
            {
                bitsPerLine = (bitsPerLine + 7) & ~7;
            }

            for (int y = 0; y < tileHeight; ++y)
            {
                int lineBitOffset = y * bitsPerLine;
                for (int x = 0; x < tileWidth; ++x)
                {
                    int pixelBitOffset = lineBitOffset + (x * samplesToTransfer * bitsPerSample);
                    for (int i = 0; i < samplesToTransfer; ++i)
                    {
                        int bitOffset = pixelBitOffset + (i * bitsPerSample);
                        int outIndex = (((y * tileWidth) + x) * samplesToTransfer) + i;

                        int byteOffset = (int)Math.Floor(bitOffset / 8.0);
                        int innerBitOffset = bitOffset % 8;
                        if (innerBitOffset + bitsPerSample <= 8)
                        {
                            int result = (view.GetUint8(byteOffset) >> (8 - bitsPerSample - innerBitOffset)) & bitMask;
                            outArray.SetValue(result,
                                outIndex); // TODO: not sure why they don't care about endianness here.
                        }
                        else if (innerBitOffset + bitsPerSample <= 16)
                        {
                            int result = (view.GetUint16(byteOffset) >> (16 - bitsPerSample - innerBitOffset)) &
                                         bitMask;
                            outArray.SetValue(result,
                                outIndex); // TODO: not sure why they don't care about endianness here.
                        }
                        else if (innerBitOffset + bitsPerSample <= 24)
                        {
                            int raw = (view.GetUint16(byteOffset) << 8) | view.GetUint8(byteOffset + 2);
                            int result = (raw >> (24 - bitsPerSample - innerBitOffset)) & bitMask;
                            outArray.SetValue(result,
                                outIndex); // TODO: not sure why they don't care about endianness here.
                        }
                        else
                        {
                            long result = (view.GetUint32(byteOffset) >> (32 - bitsPerSample - innerBitOffset)) &
                                          bitMask;
                            outArray.SetValue((int)result,
                                outIndex); // TODO: fix narrowing conversion // TODO: not sure why they don't care about endianness here.
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
        return fileDirectory.GetGeoDirectoryValue<int?>("GeographicTypeGeoKey");
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GeoTIFFReadResult<T>> ReadValueAtCoordinateAsync<T>(double x, double y,
        CancellationToken? cancellationToken = null) where T : struct
    {
        IEnumerable<double>? modelTransformationList =
            fileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
        if (modelTransformationList is not null)
        {
            throw new NotImplementedException("Model transformations not yet supported");
        }

        //TODO: Check not out of bounds
        VectorXYZ origin = GetOrigin();
        Tuple<double, double, double> res = GetResolution();
        // If the user passed a low x, we want to be close to the orgin.
        double left = (x - origin.X) / res.Item1;
        double right = left + res.Item1;

        // if the user passed a low y, be far away from the origin.

        double top = (y - origin.Y) / res.Item2;
        double bottom = top + 1;

        var window = new ImageWindow()
        {
            Left = (uint)left, Right = (uint)right, Bottom = (uint)bottom, Top = (uint)top
        };

        return await ReadRastersAsync<T>(window, cancellationToken);
    }
}