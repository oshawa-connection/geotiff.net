using Geotiff.Compression;
using Geotiff.Exceptions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff;

public class GeoTiffImage
{
    public readonly ImageFileDirectory FileDirectory;
    public readonly bool littleEndian;
    private readonly bool cache;
    private readonly BaseSource source;
    private readonly Dictionary<int, ArrayBuffer>? tiles;
    private readonly bool isTiled;
    private readonly ushort planarConfiguration;
    public GeoTiffImage(ImageFileDirectory fileDirectory, bool littleEndian, bool cache, BaseSource source)
    {
        this.FileDirectory = fileDirectory;
        this.littleEndian = littleEndian;
        tiles = cache ? new Dictionary<int, ArrayBuffer>() : null;

        isTiled = fileDirectory.TagDictionary.ContainsKey("StripOffsets") is false;
        ushort? planarConfiguration = fileDirectory.GetFileDirectoryValueUShortOrNull(FieldTypes.PlanarConfiguration);

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
        IEnumerable<double>? tiePoint = FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTiepoint);
        return tiePoint is not null && tiePoint.Count() == 6;
    }

    public bool HasValidModelTransformation()
    {
        IEnumerable<double>? modelTransformation =
            FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
        return modelTransformation is not null && modelTransformation.Count() > 11;
    }

    /// <summary>
    /// Can use this before checking for origin or boundingbox to prevent exceptions
    /// Very useful reference http://geotiff.maptools.org/spec/geotiff2.6.html
    /// </summary>
    /// <returns></returns>
    public bool HasAffineTransformation()
    {
        return HasValidModelTransformation() || HasValidTiePoints();
    }

    /// <summary>
    /// Returns null if there is no affine transformation set
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException">Thrown if the affine transformation is invalid</exception>
    public VectorXYZ? GetOrigin()
    {
        IEnumerable<double>? tiePoint = FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTiepoint);
        IEnumerable<double>? modelTransformation =
            FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);

        if (HasValidTiePoints())
        {
            var affine = AffineTransformation.FromTiepoint(tiePoint.ToArray());
            return affine.GetOrigin();
        }

        if (modelTransformation is not null)
        {
            var affine = AffineTransformation.FromModelTransformation(modelTransformation.ToArray());
            return affine.GetOrigin();
        }

        return null;
    }
    
    public IEnumerable<Tag> GetAllRawTags()
    {
        return this.FileDirectory.RawFileDirectory.Values;
    }
    
    /// <summary>
    /// Lists all standard, extended and GDAL tags known to this library.
    /// Unrecognised tags will be excluded, use GetAllRawTags instead for these.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Tag> GetAllKnownTags()
    {
        return this.FileDirectory.TagDictionary.Values;
    }

    /// <summary>
    /// uint return type here is confirmed by geotiff spec
    /// </summary>
    /// <returns></returns>
    public uint GetWidth()
    {
        return (uint)FileDirectory.GetFileDirectoryValueUInt(FieldTypes.ImageWidth);
    }

    /// <summary>
    /// uint return type here is confirmed by geotiff spec
    /// </summary>
    /// <returns></returns>
    public uint GetHeight()
    {
        return FileDirectory.GetFileDirectoryValueUInt(FieldTypes.ImageLength);
    }


    /// <summary>
    /// Get the resolution, or null if there is no affine transformation set.
    /// </summary>
    /// <returns></returns>
    public VectorXYZ? GetResolution()
    {
        IEnumerable<double>? modelPixelScaleR =
            FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelPixelScale);
        
        if (modelPixelScaleR is not null)
        {
            double[] modelPixelScale = modelPixelScaleR.ToArray();
            var affine = AffineTransformation.FromModelPixelScale(modelPixelScale);
            return affine.GetResolution();
        }
        
        IEnumerable<double>? modelTransformationR =
            FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
        
        if (modelTransformationR is not null)
        {
            double[] modelTransformation = modelTransformationR.ToArray();
            var affineTransformation = AffineTransformation.FromModelTransformation(modelTransformation);
            return affineTransformation.GetResolution();
        }

        return null;
    }


    /// <summary>
    /// Returns the image bounding box as an array of 4 values: min-x, min-y,
    /// max-x and max-y. Returns null when the image has no affine transformation.
    /// </summary>
    /// <param name="tilegrid">If true return extent for a tilegrid without adjustment for ModelTransformation.</param>
    /// <returns>The bounding box</returns>
    public BoundingBox? GetBoundingBox(bool tilegrid = false)
    {
        uint height = GetHeight();
        uint width = GetWidth();
        IEnumerable<double>? modelTransformationList =
            FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
        
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
            
            IEnumerable<double>? modelTransformationR =
                FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);

            if (modelTransformationR is null)
            {
                return null;
            }
            
            VectorXYZ origin = GetOrigin();
            VectorXYZ resolution = GetResolution();

            double x1 = origin.X;
            double y1 = origin.Y;

            double x2 = x1 + (resolution.X * width);
            double y2 = y1 + (resolution.Y * height);

            return new BoundingBox()
            {
                XMin = Math.Min(x1, x2), YMin = Math.Min(y1, y2), XMax = Math.Max(x1, x2), YMax = Math.Max(y1, y2)
            };
        }
    }

    
    /// <summary>
    /// Get the affine transformation for the image. If ModelPixelScaleTag+ModelTiepointTag are being used instead,
    /// calculate the affine transform and return it. If there is no ModelPixelScaleTag+ModelTiepointTag/ affine
    /// transformation set, return null. 
    /// </summary>
    /// <returns></returns>
    public AffineTransformation? GetOrCalculateAffineTransformation()
    {
        IEnumerable<double>? modelPixelScaleR =
            FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelPixelScale);
        IEnumerable<double>? tiePoint = FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTiepoint);
        IEnumerable<double>? modelTransformation =
            FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
        
        if (modelTransformation is not null)
        {
            return AffineTransformation.FromModelTransformation(modelTransformation.ToArray());
        }
        
        if (modelPixelScaleR is not null && tiePoint is not null)
        {
            return AffineTransformation.FromModelPixelScaleAndTiePoints(modelPixelScaleR.ToArray(),tiePoint.ToArray());
            
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ulong GetSamplesPerPixel()
    {
        ulong samplesPerPixel = FileDirectory.GetFileDirectoryValueULong(FieldTypes.SamplesPerPixel);
        return samplesPerPixel != 0 ? samplesPerPixel : 1;
    }


    public uint GetBitsForSample(int sampleIndex = 0)
    {
        ushort[] bitsPerSample = FileDirectory.GetFileDirectoryListValue<ushort>(FieldTypes.BitsPerSample).ToArray();
        return bitsPerSample[sampleIndex];
    }
    
    public int[] GetBitsPerSample()
    {
        int[] bitsPerSample = FileDirectory.GetFileDirectoryListValue<int>(FieldTypes.BitsPerSample).ToArray();
        return bitsPerSample;
    }


    /// <summary>
    /// Returns the number of bytes per pixel.
    /// </summary>
    public int GetBytesPerPixel()
    {
        ushort[] bitsPerSample = FileDirectory.GetFileDirectoryListValue<ushort>(FieldTypes.BitsPerSample).ToArray();
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
        ushort[] bitsPerSample = FileDirectory.GetFileDirectoryValueUShortArray(FieldTypes.BitsPerSample).ToArray();
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
            ? (uint)FileDirectory.GetFileDirectoryValueUInt(FieldTypes.TileWidth)
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
            return (uint)FileDirectory.GetFileDirectoryValueUInt(FieldTypes.TileLength);
        }

        uint? rowsPerStrip = FileDirectory.GetFileDirectoryValueUInt(FieldTypes.RowsPerStrip);
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
        IEnumerable<int>? samplesFormat = FileDirectory.GetFileDirectoryListValue<int>(FieldTypes.SampleFormat);
        return samplesFormat?.ElementAt(sampleIndex) ?? 1;
    }

    public int GetPredictor()
    {
        if (this.FileDirectory.HasTag("Predictor"))
        {
            return this.FileDirectory.GetFileDirectoryValueInt("Predictor");
        }

        return 1;
    }


    private GeotiffSampleDataType SampleDataTypeForSample(int sampleIndex)
    {
        int format = GetSampleFormat(sampleIndex);
        uint bitsPerSample = GetBitsForSample(sampleIndex);
        
        switch (format)
        {
            case 1: // unsigned integer data
                if (bitsPerSample <= 8)
                {
                    return GeotiffSampleDataType.UInt8;
                }
                else if (bitsPerSample <= 16)
                {
                    return GeotiffSampleDataType.UInt16;
                }
                else if (bitsPerSample <= 32)
                {
                    return GeotiffSampleDataType.UInt32;
                }

                break;
            case 2: // twos complement signed integer data
                switch (bitsPerSample)
                {
                    case 8:
                        return GeotiffSampleDataType.Int8;
                    case 16:
                        return GeotiffSampleDataType.Int16;
                    case 32:
                        return GeotiffSampleDataType.Int32;
                }

                break;
            case 3: // floating point data
                switch (bitsPerSample)
                {
                    case 16: // TODO: Use .net Half or not?
                    case 32:
                        return GeotiffSampleDataType.Float32;
                    case 64:
                        return GeotiffSampleDataType.Double;
                }

                break;
            default:
                break;
        }
        
        throw new InvalidTiffException("Unsupported data format/bitsPerSample");
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
                    return new DataView(size, GeotiffSampleDataType.UInt8);
                }
                else if (bitsPerSample <= 16)
                {
                    throw new NotImplementedException();
                    // return new short[size];
                }
                else if (bitsPerSample <= 32)
                {
                    return new DataView(size, GeotiffSampleDataType.UInt32);
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
                        return new DataView(size, GeotiffSampleDataType.Int32);
                }

                break;
            case 3: // floating point data
                switch (bitsPerSample)
                {
                    case 16:
                    case 32:
                        return new DataView(size, GeotiffSampleDataType.Float32);
                    case 64:
                        return new DataView(size,GeotiffSampleDataType.Double);
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
        uint bitsPerSample = GetBitsForSample(sampleIndex);
        return ArrayForType(format, bitsPerSample, size);
    }

    private Array GetArrayForSample(int sampleIndex, ArrayBuffer buffer)
    {
        int format = GetSampleFormat(sampleIndex);
        uint bitsPerSample = GetBitsForSample(sampleIndex);
        return ArrayForType(format, bitsPerSample, buffer);
    }

    public async Task<Raster> ReadRasterBoundingBoxAsync(BoundingBox boundingBox,
        IEnumerable<int>? sampleSelection = null, CancellationToken? cancellationToken = null)
    {
        var window = this.BoundingBoxToPixelWindow(boundingBox);
        return await this.ReadRasterAsync(window, sampleSelection, cancellationToken);
    }
    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window">Pixel area to read</param>
    /// <param name="sampleSelection">sample indices (0 indexed) to read</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="InvalidTiffException"></exception>
    public async Task<Raster> ReadRasterAsync(ImagePixelWindow? window = null, IEnumerable<int>? sampleSelection = null, CancellationToken? cancellationToken = null)
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
        
        ulong samplesPerPixel = GetSamplesPerPixel();
        // TODO: this will need to change when we add support for choosing the samples to be read.
        IEnumerable<int> samples =
            Enumerable.Range(0, (int)samplesPerPixel)
                .ToArray(); // TODO: change away from using Enumerable.Range here as it doesn't accept ulong, or write extension.
        
        if (sampleSelection is not null)
        {
            samples = sampleSelection.ToArray();
        }
        
        SparseList<RasterSample> valueArrays = new();
        
        for (int i = 0; i < samples.Count(); ++i)
        {
            var sampleDataType = SampleDataTypeForSample(samples.ElementAt(i));
            valueArrays[samples.ElementAt(i)] = new RasterSample(imageWindowWidth, imageWindowHeight, this, sampleDataType, (int)numPixels);
        }
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

        SparseList<int> srcSampleOffsets = new();
        for (int i = 0; i < samples.Count(); ++i)
        {
            if (planarConfiguration == 1)
            {
                srcSampleOffsets.Add(samples.ElementAt(i),sum(this.FileDirectory.BitsPerSample, 0, samples.ElementAt(i)) / 8);
            }
            else
            {
                srcSampleOffsets.Add(samples.ElementAt(i),0);
            }
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
                for (int sampleIndex = 0; sampleIndex < samples.Count(); ++sampleIndex)
                {
                    int sample = samples.ElementAt(sampleIndex);
                    if (planarConfiguration == 2)
                    {
                        getPromise = GetTileOrStripAsync(xTile, yTile, sample, new DecoderRegistry(),
                            cancellationToken);
                    }

                    Task<bool> promise = getPromise.Then<TileOrStripResult, bool>(sample,(tile, si) =>
                    {
                        ArrayBuffer buffer = tile.data;
                        
                        var dataView = new DataView(buffer);
                        long blockHeight = GetBlockHeight(tile.y);
                        long firstLine = tile.y * tileHeight;
                        long firstCol = tile.x * tileWidth;
                        long lastLine = firstLine + blockHeight;
                        long lastCol = (tile.x + 1) * tileWidth;

                        long ymax = JSMath.Min(blockHeight, blockHeight - (lastLine - imageWindow[3]),
                            imageHeight - firstLine);
                        ulong xmax = JSMath.Min((ulong)tileWidth, (ulong)(tileWidth - (lastCol - imageWindow[2])),
                            (ulong)(imageWidth - firstCol));

                        for (long y = Math.Max(0, imageWindow[1] - firstLine); y < ymax; ++y)
                        {
                            for (long x = Math.Max(0, imageWindow[0] - firstCol); (ulong)x < xmax; ++x)
                            {
                                var bytesPerPixelToUse = bytesPerPixel;
                                if (planarConfiguration == 2)
                                {
                                    bytesPerPixelToUse = GetSampleByteSize(si);
                                }
                                long pixelOffset = ((y * tileWidth) + x) * bytesPerPixelToUse;
                                long windowCoordinate = (
                                    (y + firstLine - imageWindow[1]) * windowWidth
                                ) + x + firstCol - imageWindow[0];

                                int format = FileDirectory.SampleFormat is not null
                                    ? FileDirectory.SampleFormat[si]
                                    : 1;
                                int bitsPerSample = FileDirectory.BitsPerSample[si];

                                var myArray = valueArrays[si];
                                var dv = dataView;
                                
                                switch (format)
                                {
                                    case 1: // unsigned integer data
                                        if (bitsPerSample <= 8)
                                        {
                                            var read = dv.GetUint8((int)pixelOffset + srcSampleOffsets[si]);
                                            myArray.SetUInt8(read, (int)windowCoordinate);
                                        }
                                        else if (bitsPerSample <= 16)
                                        {
                                            var read = dv.GetUint16((int)pixelOffset + srcSampleOffsets[si],
                                                littleEndian);
                                            myArray.SetUInt16(read, (int)windowCoordinate);
                                        }
                                        else if (bitsPerSample <= 32)
                                        {
                                            var read = dv.GetUint32((int)pixelOffset + srcSampleOffsets[si],
                                                littleEndian);
                                            myArray.SetUInt32(read, (int)windowCoordinate);
                                            // return (dv, offset, endianNess) => dv.GetUint32((int)offset, endianNess);
                                        }

                                        break;
                                    case 2: // twos complement signed integer data
                                        if (bitsPerSample <= 8)
                                        {
                                            var read = dv.GetInt8((int)pixelOffset + srcSampleOffsets[si]);
                                            myArray.SetInt8(read, (int)windowCoordinate);
                                            // return (dv, offset, endianNess) => dv.GetInt8((int)offset);
                                        }
                                        else if (bitsPerSample <= 16)
                                        {
                                            var read = dv.GetInt16((int)pixelOffset + srcSampleOffsets[si], littleEndian);
                                            myArray.SetInt16(read, (int)windowCoordinate);
                                            // return (dv, offset, endianNess) => dv.GetInt16((int)offset, endianNess);
                                        }
                                        else if (bitsPerSample <= 32)
                                        {
                                            var read = dv.GetInt32((int)pixelOffset + srcSampleOffsets[si], littleEndian);
                                            myArray.SetInt32(read, (int)windowCoordinate);
                                        }

                                        break;
                                    case 3:
                                        switch (bitsPerSample)
                                        {
                                            case 16: // TODO: Use dotnet Half type here?
                                                throw new NotImplementedException();
                                            case 32:
                                                var read1 = dv.GetFloat32((int)pixelOffset +
                                                                          srcSampleOffsets[si], littleEndian);
                                                myArray.SetFloat32(read1, (int)windowCoordinate);
                                                break;
                                            case 64:
                                                var read2 = dv.GetFloat64((int)pixelOffset +
                                                                          srcSampleOffsets[si], littleEndian);
                                                myArray.SetDouble(read2, (int)windowCoordinate);
                                                break;
                                            default:
                                                throw new InvalidTiffException("Unsupported data format/bitsPerSample");
                                        }

                                        break;
                                    default:
                                        throw new InvalidTiffException("Unsupported data format/bitsPerSample");
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
        
        return new Raster(valueArrays,this.GetOrCalculateAffineTransformation(), imageWidth, imageHeight, this);
    }
    
    private int sum(IEnumerable<int> array, int start, int end) {
        var s = 0;
        for (var i = start; i < end; ++i) {
            s += array.ElementAt(i);
        }
        return s;
    }
    
    /// <summary>
    /// Technically TIFF does support different types for each sample, but almost no software/ tooling supports this.
    /// (including geotiff.NET for that matter). If your use case requires this please file an issue on GitHub.
    /// </summary>
    /// <param name="sampleIndex"></param>
    /// <returns></returns>
    public GeotiffSampleDataType GetSampleType(int sampleIndex = 0)
    {
        int format = FileDirectory.SampleFormat is not null
            ? FileDirectory.SampleFormat[sampleIndex]
            : 1;
        int bitsPerSample = FileDirectory.BitsPerSample[sampleIndex];
        switch (format)
        {
            case 1: // unsigned integer data
                if (bitsPerSample <= 8)
                {
                    return GeotiffSampleDataType.UInt8;
                }
                else if (bitsPerSample <= 16)
                {
                    return GeotiffSampleDataType.UInt16;
                }
                else if (bitsPerSample <= 32)
                {
                    return GeotiffSampleDataType.UInt32;
                }

                break;
            case 2: // twos complement signed integer data
                if (bitsPerSample <= 8)
                {
                    return GeotiffSampleDataType.Int8;
                }
                else if (bitsPerSample <= 16)
                {
                    return GeotiffSampleDataType.Int16;
                }
                else if (bitsPerSample <= 32)
                {
                    return GeotiffSampleDataType.Int32;
                }

                break;
            case 3:
                switch (bitsPerSample)
                {
                    case 16:
                        throw new NotImplementedException();
                    case 32:
                        return GeotiffSampleDataType.Float32;
                    case 64:
                        return GeotiffSampleDataType.Double;
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
        int format = FileDirectory.SampleFormat is not null
            ? FileDirectory.SampleFormat[sampleIndex]
            : 1;
        int bitsPerSample = FileDirectory.BitsPerSample[sampleIndex];
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
                    case 16: // TODO: Use dotnet Half type here?
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


    /// <summary>
    /// Returns the decoded strip or tile.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sample"></param>
    /// <param name="poolOrDecoder"></param>
    /// <param name="signal"></param>
    /// <returns></returns>
    private async Task<TileOrStripResult> GetTileOrStripAsync(int x, int y, int sample, DecoderRegistry poolOrDecoder,
        CancellationToken? signal)
    {
        int numTilesPerRow = (int)Math.Ceiling((int)GetWidth() / (double)GetTileWidth());
        int numTilesPerCol = (int)Math.Ceiling((double)GetHeight() / (double)GetTileHeight());
        int index = 0;
        var sampleToUse = 0;
        if (planarConfiguration == 1)
        {
            index = (y * numTilesPerRow) + x;
        }
        else if (planarConfiguration == 2)
        {
            sampleToUse = sample;
            index = (sampleToUse * numTilesPerRow * numTilesPerCol) + (y * numTilesPerRow) + x;
        }

        int offset;
        int byteCount;
        if (isTiled)
        {
            offset = FileDirectory.GetFileDirectoryListValue<int>("TileOffsets").ElementAt(index);
            byteCount = FileDirectory.GetFileDirectoryListValue<int>("TileByteCounts").ElementAt(index);
        }
        else
        {
            offset = FileDirectory.GetFileDirectoryListValue<int>("StripOffsets").ElementAt(index);
            byteCount = FileDirectory.GetFileDirectoryListValue<int>("StripByteCounts").ElementAt(index);
        }

        if (byteCount == 0)
        {
            long nPixels = GetBlockHeight(y) * GetTileWidth();
            int bytesPerPixel = planarConfiguration == 2
                ? GetSampleByteSize(sampleToUse)
                : GetBytesPerPixel();
            var data = new ArrayBuffer(nPixels * bytesPerPixel);
            Array view = GetArrayForSample(sampleToUse, data);

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

            return new TileOrStripResult { x = x, y = y, data = data};
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
                var z = x + y + sampleToUse;
                int sampleFormat = GetSampleFormat();
                uint bitsForCurrentSample = GetBitsForSample(); // TODO: pass sample index here; works right now because most tiffs only contain one sample type. 
                var bitsPerSample = GetBitsPerSample();
                ArrayBuffer data = await poolOrDecoder.DecodeAsync(FileDirectory, slice, (int)GetTileWidth(), (int)GetTileHeight(), GetPredictor(), bitsPerSample, planarConfiguration);
                
                if (NeedsNormalization(sampleFormat, (int)bitsForCurrentSample))
                {
                    data = NormalizeArray(
                        data,
                        sampleFormat,
                        planarConfiguration,
                        (int)GetSamplesPerPixel(),
                        (int)bitsForCurrentSample,
                        (int)GetTileWidth(), // TODO: Why not block width?
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
        return new TileOrStripResult() { x = x, y = y, data = finalData};
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
        if (FileDirectory.GDAL_NODATA == null)
        {
            return null;
        }

        string? str = FileDirectory.GDAL_NODATA;
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
    public short? GetProjectionString()
    {
        return FileDirectory.GetGeoDirectoryValue<short?>("GeographicTypeGeoKey");
    }
    
    /// <summary>
    /// Returns null if the affine transformation is not set.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sampleSelection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Raster?> ReadPixelSamplesAtCoordinateAsync(double x, double y, IEnumerable<int>? sampleSelection = null, CancellationToken? cancellationToken = null)
    {
        var affine = this.GetOrCalculateAffineTransformation();
        if (affine is null)
        {
            return null;
        }
        
        var pixelOrigin = affine.ModelToPixel(x, y);
        
        //TODO: Check not out of bounds
        VectorXYZ origin = GetOrigin();

        if (origin is null)
        {
            return null;
        }
        
        VectorXYZ res = GetResolution();
        // If the user passed a low x, we want to be close to the origin.
        double left = (x - origin.X) / res.X;
        double right = left + res.X;

        // if the user passed a low y, be far away from the origin.

        double top = (y - origin.Y) / res.Y;
        double bottom = top + 1; 

        var window = new ImagePixelWindow()
        {
            Left = (uint)left, 
            Right = (uint)right, 
            Bottom = (uint)bottom, 
            Top = (uint)top
        };

        return await ReadRasterAsync(window, sampleSelection, cancellationToken);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ImagePixelWindow BoundingBoxToPixelWindow(BoundingBox bbox)
    {
        IEnumerable<double>? modelTransformationList =
            FileDirectory.GetFileDirectoryListValue<double>(FieldTypes.ModelTransformation);
        
        if (modelTransformationList is not null)
        {
            throw new NotImplementedException("Model transformations not yet supported");
        }

        //TODO: Check not out of bounds
        VectorXYZ origin = GetOrigin();
        VectorXYZ res = GetResolution();
        // If the user passed a low x, we want to be close to the origin.
        double left = (bbox.XMin - origin.X) / res.X;
        double right = (bbox.XMax - origin.X) / res.X;

        // if the user passed a low y, be far away from the origin.

        double top = (bbox.YMax - origin.Y) / res.Y;
        double bottom = (bbox.YMin - origin.Y) / res.Y;

        return new ImagePixelWindow()
        {
            Left = (uint)left, 
            Right = (uint)right, 
            Bottom = (uint)bottom, 
            Top = (uint)top
        };
    }
}