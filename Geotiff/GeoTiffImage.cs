using Geotiff.Compression;
using Geotiff.Exceptions;
using Geotiff.Interfaces;
using Geotiff.JavaScriptCompatibility;
using System.Xml.Linq;

namespace Geotiff;

/// <summary>
/// A GeoTiff comprises multiple GeoTiffImage's (called sub-datasets). Most of the time, there is just one sub-dataset.
/// </summary>
public class GeoTiffImage : IGetTagable, IReadRasterable
{
    private readonly ImageFileDirectory _fileDirectory;
    private readonly GeoTiff _parentFile;
    private readonly bool _littleEndian;
    private readonly BaseSource _source;
    private readonly Dictionary<ulong, byte[]>? _tileCache;
    private readonly bool _isTiled;
    private readonly ushort _planarConfiguration;
    private ulong[]? _stripOffsetsCached;
    private ulong[]? _stripByteCountsCached;
    
    private ulong[]? _tileOffsetsCached;
    private ulong[]? _tileByteCountsCached;
    
    private ushort[]? _bitsPerSampleCached;
    
    private byte[]? _jpegTablesCached;

    public GeoTiffImage(ImageFileDirectory fileDirectory)
    {
        this._fileDirectory = fileDirectory;
        this._littleEndian = true;
    }
    
    internal GeoTiffImage(GeoTiff parentFile, ImageFileDirectory fileDirectory, bool littleEndian, bool cache, BaseSource source)
    {
        this._parentFile = parentFile;
        this._fileDirectory = fileDirectory;
        this._littleEndian = littleEndian;
        _tileCache = cache ? new Dictionary<ulong, byte[]>() : null;

        _isTiled = fileDirectory.TagDictionary.ContainsKey("StripOffsets") is false;
        var planarConfigurationTag = GetTag(TagFields.PlanarConfiguration);
        
        if (planarConfigurationTag is null)
        {
            this._planarConfiguration = 1;
        }
        else
        {
            this._planarConfiguration = (ushort)planarConfigurationTag.GetUShort();
        }

        if (this._planarConfiguration != 1 && this._planarConfiguration != 2)
        {
            throw new InvalidGeoTiffException("Invalid planar configuration.");
        }

        this._source = source;
        var _ = this.JpegTables;// Populate this to cache it before decoding starts
    }
    

    
    /// <summary>
    /// Checks the ModelTiepoint is set and valid. According to spec: The ModelTiepointTag SHALL have type = DOUBLE
    /// and The ModelTiepointTag SHALL have 6 values for each of the tiepoints
    /// </summary>
    /// <returns></returns>
    public bool HasValidTiePoints()
    {
        var tiePoint = _fileDirectory.GetTag(TagFields.ModelTiepoint);
        if (tiePoint is null)
        {
            return false;
        }
        //The ModelTiepointTag SHALL have type = DOUBLE
        //The ModelTiepointTag SHALL have 6 values for each of the tiepoints
        return tiePoint.GetDoubleArray().Count() % 6 == 0;
    }

    public bool HasValidModelTransformation()
    {
        var modelTag = this.GetTag(TagFields.ModelTransformation);
        if (modelTag is null)
        {
            return false;
        }
        
        return modelTag.GetDoubleArray().Count() > 11;
    }

    /// <summary>
    /// 
    /// 
    /// </summary>
    /// <returns></returns>
    public bool HasAffineTransformation()
    {
        return HasValidModelTransformation() || HasValidTiePoints();
    }

    /// <summary>
    /// Derives origin from either the affine transformation or from tiepoints.
    /// Returns null if there is no affine transformation set
    /// </summary>
    /// <returns></returns>
    /// <exception cref="GeoTiffException">Thrown if the affine transformation is invalid</exception>
    public VectorXYZ? GetOrigin()
    {
        var tiepointtag = GetTag(TagFields.ModelTiepoint);
        var modelTransformationTag = this.GetTag(TagFields.ModelTransformation);
        
        if (HasValidTiePoints())
        {
            var affine = AffineTransformation.FromTiepoint(tiepointtag.GetDoubleArray());
            return affine.GetOrigin();
        }

        if (modelTransformationTag is not null)
        {
            var affine = AffineTransformation.FromModelTransformation(modelTransformationTag.GetDoubleArray());
            return affine.GetOrigin();
        }

        return null;
    }
    
    /// <summary>
    /// Lists all standard, extended and GDAL tags known to this library, as well as custom tags
    /// without names known to this library.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Tag> GetAllRawTags()
    {
        return this._fileDirectory.RawFileDirectory.Values;
    }
    
    /// <summary>
    /// Lists all standard, extended and GDAL tags known to this library.
    /// Unrecognised tags will be excluded, use GetAllRawTags instead for these.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Tag> GetAllKnownTags()
    {
        return this._fileDirectory.TagDictionary.Values;
    }
    
    

    /// <summary>
    /// Returns null if the tag is not found in the ImageFileDirectory.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Tag? GetTag(int id)
    {
        return this._fileDirectory.GetTag(id);
    }
    
    /// <summary>
    /// Returns null if the tag is not found in the ImageFileDirectory.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Tag? GetTag(string name)
    {
        return this._fileDirectory.GetTag(name);
    }
    
    internal Tag GetTagRequired(string name)
    {
        var found = this._fileDirectory.GetTag(name);
        if (found is null)
        {
            throw new InvalidGeoTiffException($"Tag '{name}' not found.");
        }
        return found;
    }

    /// <summary>
    /// Check for presence of tag by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool HasTag(string name)
    {
        return this.GetTag(name) is not null;
    }

    /// <summary>
    /// Check for presence of tag by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool HasTag(int id)
    {
        return this.GetTag(id) is not null;
    }

    public Tag GetGeoTag(string name)
    {
        return this._fileDirectory.GetGeoTag(name);
    }

    /// <summary>
    /// Returns null if GDAL_METADATA tag is not set.
    /// Will throw if there are duplicated entries.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string>? GetGDALMetadataAsDictionary()
    {
        var gdalMetadataTag = this.GetTag("GDAL_METADATA");
        if (gdalMetadataTag is null)
        {
            return null;
        }
        
        var s = gdalMetadataTag.GetString();
        
        XDocument doc = XDocument.Parse(s);

        return doc.Descendants("Item")
            .Select(d => new KeyValuePair<string, string>(d.FirstAttribute.Value, d.Value))
            .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Get the width of the image in pixels
    /// </summary>
    /// <returns></returns>
    public ulong Width
    {
        get
        {
            var imageWidthTag = this.GetTagRequired(TagFields.ImageWidth);
            return imageWidthTag.GetAsULong();
        }
    }

    /// <summary>
    /// Get the height of the image in pixels
    /// </summary>
    /// <returns></returns>
    public ulong Height
    {
        get
        {
            var imageLengthTag = this.GetTagRequired(TagFields.ImageLength);
            return imageLengthTag.GetAsULong();    
        }
    }


    /// <summary>
    /// Get the resolution, or null if there is no affine transformation set.
    /// </summary>
    /// <returns></returns>
    public VectorXYZ? GetResolution()
    {
        var modelPixelScaleTag = GetTag(TagFields.ModelPixelScale);
        
        if (modelPixelScaleTag is not null)
        {
            double[] modelPixelScale = modelPixelScaleTag.GetDoubleArray();
            var affine = AffineTransformation.FromModelPixelScale(modelPixelScale);
            return affine.GetResolution();
        }

        var modelTransformationR = GetTag(TagFields.ModelTransformation);
        
        if (modelTransformationR is not null)
        {
            double[] modelTransformation = modelTransformationR.GetDoubleArray();
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
        var height = Height;
        var width = Width;

        var modelTransformationList = GetTag(TagFields.ModelTransformation);
        
        if (modelTransformationList is not null && !tilegrid)
        {
            ModelTransformation mt = ModelTransformation.FromDoubleArray(modelTransformationList.GetDoubleArray());
            
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
            if (origin is null)
            {
                return null;
            }
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
        var modelTransformationTag = GetTag(TagFields.ModelTransformation);
        var modelPixelScaleR = GetTag(TagFields.ModelPixelScale);
        var tiePointTag = GetTag(TagFields.ModelTiepoint);
        
        if (modelTransformationTag is not null)
        {
            return AffineTransformation.FromModelTransformation(modelTransformationTag.GetDoubleArray());
        }
        
        if (modelPixelScaleR is not null && tiePointTag is not null)
        {
            return AffineTransformation.FromModelPixelScaleAndTiePoints(modelPixelScaleR.GetDoubleArray(),tiePointTag.GetDoubleArray());
        }
        return null;
    }

    /// <summary>
    /// Get the number of samples (aka channels or bands) from the image.
    /// </summary>
    /// <returns></returns>
    public int GetNumberOfSamples()
    {
        return this.SamplesPerPixel;
    }
    
    /// <summary>
    /// The number of samples stored per pixel.
    /// </summary>
    /// <returns></returns>
    public ushort SamplesPerPixel
    {
        get
        {
            var tag = GetTag(TagFields.SamplesPerPixel);
            if (tag is null)
            {
                return 1;
            }
            return tag.GetUShort();   
        }
    }
    
    /// <summary>
    /// Helper method over BitsPerSample tag; see BitsPerSample property.
    /// </summary>
    /// <param name="sampleIndex"></param>
    /// <returns></returns>
    public ushort GetBitsForSample(int sampleIndex)
    {
        ushort[] bitsPerSample = GetTagRequired(TagFields.BitsPerSample).GetUShortArray();
        return bitsPerSample[sampleIndex];
    }
    
    /// <summary>
    /// Getter that is cached for performance reasons.
    /// </summary>
    public ushort[] BitsPerSample
    {
        get
        {
            if (_bitsPerSampleCached is null)
            {
                var tag = GetTagRequired(TagFields.BitsPerSample);
                var bitsPerSampleArray = tag.GetUShortArray();
                _bitsPerSampleCached = bitsPerSampleArray;
            }

            return _bitsPerSampleCached;
        }
    }
    
    private ushort[]? sampleFormatCached;
    
    /// <summary>
    /// Getter that is cached for performance reasons.
    /// </summary>
    public ushort[]? SampleFormat
    {
        get
        {
            if (sampleFormatCached is null)
            {
                var sampleFormatTag = GetTag(TagFields.SampleFormat);
                if (sampleFormatTag is not null)
                {
                    sampleFormatCached = sampleFormatTag.GetUShortArray();    
                }
            }

            return sampleFormatCached;
        }
    }
    
    /// <summary>
    /// Getter that is cached for performance reasons.
    /// These bytes are inserted into the buffer before decoding of JPEG encoded data.
    /// </summary>
    public byte[]? JpegTables
    {
        get
        {
            if (_jpegTablesCached is null)
            {
                var tag = GetTag("JPEGTables");
                if (tag is null)
                {
                    return null;
                }
                
                _jpegTablesCached = tag.GetByteArray();
            }

            return _jpegTablesCached;
        }
    }
    
    /// <summary>
    /// Returns the raw string value of GDAL_NODATA tag, or null if it is not set. 
    /// </summary>
    public string? GetGdalNoData()
    {
        var gdalNoDataTag = GetTag("GDAL_NODATA");
        if (gdalNoDataTag is null)
        {
            return null;
        }
        return gdalNoDataTag.GetString();
    }
    
    /// <summary>
    /// Arrangement of band data.
    /// Either 1 (for pixel/contiguous/ interleaving) or 2 (chunky/ band/ separate).
    /// E.g. for a 3 band RGB tif with planarconfiguration 1, data will be arranged like this within a pixel: RGB RGB
    /// Whereas for planarconfiguration 2, it will be arranged: RR GG BB 
    /// </summary>
    /// <returns></returns>
    public ushort GetPlanarConfiguration()
    {
        return this._planarConfiguration;
    }

    /// <summary>
    /// Returns the number of bytes per pixel.
    /// </summary>
    public ulong GetNumberOfBytesPerPixel()
    {
        var bitsPerSample = BitsPerSample;
        ulong bytes = 0;
        for (int i = 0; i < bitsPerSample.Length; ++i)
        {
            bytes += GetSampleByteSize(i);
        }

        return bytes;
    }

    /// <summary>
    /// Returns the byte size of a sample at the given index.
    /// </summary>
    public ulong GetSampleByteSize(int i)
    {
        var bitsPerSample = GetTagRequired(TagFields.BitsPerSample).GetAsULongArray();
        if (i >= bitsPerSample.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(i), $"Sample index {i} is out of range.");
        }

        return (ulong)Math.Ceiling(bitsPerSample[i] / 8.0);
    }

    
    /// <summary>
    /// Returns the width of each tile or strip
    /// </summary>
    /// <returns>The width of each tile</returns>
    public ulong GetTileOrStripWidth()
    {
        var tileWidthTag = GetTag(TagFields.TileWidth);
        if (tileWidthTag is not null)
        {
            return tileWidthTag.GetAsULong();
        }

        return Width;
    }

    /// <summary>
    /// Returns the height of each tile or strip
    /// </summary>
    /// <returns>The height of each tile</returns>
    public ulong GetTileOrStripHeight()
    {
        if (_isTiled)
        {
            var tileLengthTag = GetTagRequired(TagFields.TileLength);
            return tileLengthTag.GetAsULong();
        }
        
        var rowsPerStripTag = GetTag(TagFields.RowsPerStrip);

        var imageHeight = Height;
        
        if (rowsPerStripTag is not null)
        {
            return Math.Min(rowsPerStripTag.GetAsULong(), imageHeight);
        }

        return imageHeight;// file has only one tile/ strip.
    }
    
    private ushort GetSampleFormat(int sampleIndex = 0)
    {
        var sampleFormatTag = GetTag(TagFields.SampleFormat);
        if (sampleFormatTag is null)
        {
            return 1;
        }
        return sampleFormatTag.GetUShortArray()[sampleIndex];
    }

    /// <summary>
    /// Used during decompression. Valid values are 1, 2, or 3.
    /// </summary>
    /// <returns></returns>
    public ushort GetPredictor()
    {
        var predictor = this.GetTag(TagFields.Predictor);
        if (predictor is null)
        {
            return 1;    
        }

        return predictor.GetUShort();
    }
    
    /// <summary>
    /// Read TileOffsets tag. Result is cached for performance reasons.
    /// KEEP THIS PRIVATE FOR NOW. For larger tifs, we won't want to read the entire tag into memory at once.
    /// </summary>
    /// <returns></returns>
    private ulong[] GetTileOffsets()
    {
        if (_tileOffsetsCached is not null)
        {
            return _tileOffsetsCached;
        }
        
        var stripOffsetsTag = GetTagRequired(TagFields.TileOffsets);
        _tileOffsetsCached = stripOffsetsTag.GetAsULongArray();
        return _tileOffsetsCached;
    }
    
    /// <summary>
    /// KEEP THIS PRIVATE FOR NOW. For larger tifs, we won't want to read the entire tag into memory at once.
    /// </summary>
    /// <returns></returns>
    private ulong[] GetTileByteCounts()
    {
        if (_tileByteCountsCached is not null)
        {
            return _tileByteCountsCached;
        }
        
        var tileByteCountsTag = GetTag(TagFields.TileByteCounts);
        _tileByteCountsCached = tileByteCountsTag.GetAsULongArray();
        return _tileByteCountsCached;
    }
    
    
    /// <summary>
    /// StripOffsets can be either a ushort or a ulong so make sure to cast it.
    /// KEEP THIS PRIVATE FOR NOW. For larger tifs, we won't want to read the entire tag into memory at once.
    /// </summary>
    /// <returns></returns>
    private ulong[] GetStripOffsets()
    {
        if (_stripOffsetsCached is not null)
        {
            return _stripOffsetsCached;
        }
        
        var stripOffsetsTag = GetTagRequired(TagFields.StripOffsets);
        _stripOffsetsCached = stripOffsetsTag.GetAsULongArray();
        return _stripOffsetsCached;
    }

    /// <summary>
    /// KEEP THIS PRIVATE FOR NOW. For larger tifs, we won't want to read the entire tag into memory at once.
    /// </summary>
    /// <returns></returns>
    private ulong[] GetStripByteCounts()
    {
        if (_stripByteCountsCached is not null)
        {
            return _stripByteCountsCached;
        }
        
        var stripByteCountsTag = GetTag(TagFields.StripByteCounts);
        _stripByteCountsCached = stripByteCountsTag.GetAsULongArray();
        return _stripByteCountsCached;
    }

    
    private GeotiffSampleDataType SampleDataTypeForSample(int sampleIndex)
    {
        int format = GetSampleFormat(sampleIndex);
        ushort bitsPerSample = GetBitsForSample(sampleIndex);
        
        switch (format)
        {
            case 1: // unsigned integer data
                switch (bitsPerSample)
                {
                    case <= 8: // Could be 1-bit sample
                        return GeotiffSampleDataType.UInt8;
                    case 16:
                        return GeotiffSampleDataType.UInt16;
                    case 32:
                        return GeotiffSampleDataType.UInt32;
                    case 64:
                        return GeotiffSampleDataType.UInt64;
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
                    case 64:
                        return GeotiffSampleDataType.Int64;
                }

                break;
            case 3: // floating point data
                switch (bitsPerSample)
                {
                    case 16:
                        return GeotiffSampleDataType.Float16;
                    case 32:
                        return GeotiffSampleDataType.Float32;
                    case 64:
                        return GeotiffSampleDataType.Float64;
                }

                break;
        }
        
        throw new InvalidGeoTiffException("Unsupported data format/bitsPerSample");
    }
    
    /// <summary>
    /// Returns null if the affine transformation is not set.
    /// </summary>
    /// <param name="boundingBox"></param>
    /// <param name="sampleSelection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Raster> ReadRasterBoundingBoxAsync(BoundingBox boundingBox,
        IEnumerable<int>? sampleSelection = null, CancellationToken? cancellationToken = null)
    {
        var window = this.BoundingBoxToPixelWindow(boundingBox);
        if (boundingBox is null)
        {
            return null;
        }
        return await this.ReadRasterAsync(window, sampleSelection, cancellationToken);
    }
    
    
    /// <summary>
    /// Returns null if the affine transformation is not set.
    /// </summary>
    /// <param name="boundingBox"></param>
    /// <param name="sampleSelection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Raster> ReadRasterMaskedBoundingBoxAsync(BoundingBox boundingBox,
        IEnumerable<int>? sampleSelection = null, CancellationToken? cancellationToken = null)
    {
        var window = this.BoundingBoxToPixelWindow(boundingBox);
        if (boundingBox is null)
        {
            return null;
        }
        return await this.ReadRasterMaskedAsync(window, sampleSelection, cancellationToken);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="window">Pixel area to read</param>
    /// <param name="sampleSelection">sample indices (0 indexed) to read</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="GeoTiffException"></exception>
    /// <exception cref="InvalidGeoTiffException"></exception>
    public async Task<Raster> ReadRasterAsync(
        ImagePixelWindow? window = null, 
        IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null)
    {
        ulong[] imageWindow = new ulong[] { 0, 0, Width, Height };

        if (window is not null)
        {
            imageWindow = window.ToArray();
        }

        if (imageWindow[0] > imageWindow[2] || imageWindow[1] > imageWindow[3])
        {
            throw new GeoTiffException("Invalid image window");
        }

        ulong imageWindowWidth = imageWindow[2] - imageWindow[0];
        ulong imageWindowHeight = imageWindow[3] - imageWindow[1];

        ulong numPixels =
            (ulong)imageWindowWidth * (ulong)imageWindowHeight; // ignore resharper telling you that cast is redundant.
        
        IEnumerable<int> samples =
            Enumerable.Range(0, (int)SamplesPerPixel)
                .ToArray(); 
        
        if (sampleSelection is not null)
        {
            samples = sampleSelection.ToArray();
        }
        
        SparseList<RasterSample> rasterSamples = new();
        
        for (int i = 0; i < samples.Count(); ++i)
        {
            var sampleDataType = SampleDataTypeForSample(samples.ElementAt(i));
            rasterSamples[samples.ElementAt(i)] = new RasterSample(imageWindowWidth, imageWindowHeight, this,  sampleDataType, (int)numPixels);
        }

        var blockInfo = GetBlockInfo(imageWindow);
        
        var tileWidth = GetTileOrStripWidth();
        var tileHeight = GetTileOrStripHeight();
        
        var imageWidth = Width;
        var imageHeight = Height;
        
        ulong minXTile = blockInfo.MinXTile;
        ulong maxXTile = blockInfo.MaxXTile;
        ulong minYTile = blockInfo.MinYTile;
        ulong maxYTile = blockInfo.MaxYTile;
        
        var windowWidth = imageWindow[2] - imageWindow[0];

        ulong bytesPerPixel = GetNumberOfBytesPerPixel();

        SparseList<int> srcSampleOffsets = new();
        for (int i = 0; i < samples.Count(); ++i)
        {
            if (_planarConfiguration == 1)
            {
                srcSampleOffsets.Add(samples.ElementAt(i),sum(BitsPerSample, 0, samples.ElementAt(i)) / 8);
            }
            else
            {
                srcSampleOffsets.Add(samples.ElementAt(i),0);
            }
        }

        // Setup cached values for either strips or tiles.
        // Long term todo item would be to not read the entire array; for particularly large tiffs the offsets will be giant.
        if (_isTiled is false)
        {
            GetStripOffsets();
            GetStripByteCounts();
        }
        else
        {
            GetTileOffsets();
            GetTileByteCounts();
        }
        
        var promises = new List<Task>();
        
        for (ulong yTile = minYTile; yTile < maxYTile; ++yTile)
        {
            for (ulong xTile = minXTile; xTile < maxXTile; ++xTile)
            {
                Task<TileOrStripResult>? getPromise = null;
                if (_planarConfiguration == 1)
                {
                    getPromise = GetTileOrStripAsync(xTile, yTile, 0, cancellationToken);
                }
                for (int sampleIndex = 0; sampleIndex < samples.Count(); ++sampleIndex)
                {
                    int sample = samples.ElementAt(sampleIndex);
                    if (_planarConfiguration == 2)
                    {
                        getPromise = GetTileOrStripAsync(xTile, yTile, sample,
                            cancellationToken);
                    }

                    Task<bool> promise = getPromise.JSThen<TileOrStripResult, bool>(sample,(tile, si) =>
                    {
                        byte[] buffer = tile.data;
                        
                        var dataView = new DataView(buffer);
                        ulong blockHeight = GetBlockHeight(tile.y);
                        ulong firstLine = tile.y * tileHeight;
                        ulong firstCol = tile.x * tileWidth;
                        ulong lastLine = firstLine + blockHeight;
                        ulong lastCol = (tile.x + 1) * tileWidth;

                        ulong ymax = JSMath.JSMin(blockHeight, blockHeight - (lastLine - imageWindow[3]),
                            imageHeight - firstLine);
                        ulong xmax = JSMath.JSMin((ulong)tileWidth, (ulong)(tileWidth - (lastCol - imageWindow[2])),
                            (ulong)(imageWidth - firstCol));

                        ulong startY = 0;
                        if (imageWindow[1] > firstLine)
                        {
                            startY = imageWindow[1] - firstLine;
                        }

                        ulong startX = 0;
                        if (imageWindow[0] > firstCol)
                        {
                            startX = imageWindow[0] - firstCol;
                        }
                        
                        var sampleSetCallback = RasterSample.SetUInt8DataView;
                        
                        ulong bytesPerPixelToUse = bytesPerPixel;
                        if (_planarConfiguration == 2)
                        {
                            bytesPerPixelToUse = GetSampleByteSize(si);
                        }
                        
                        ushort bitsPerSample = GetBitsForSample(si);
                        
                        ushort format = SampleFormat is not null
                            ? SampleFormat[si]
                            : (ushort)1;
                        
                        switch (format)
                        {
                            case 1: // unsigned integer data
                                if (bitsPerSample <= 8)
                                {
                                    sampleSetCallback = RasterSample.SetUInt8DataView;
                                }
                                else if (bitsPerSample <= 16)
                                {
                                    sampleSetCallback = RasterSample.SetUInt16DataView;
                                }
                                else if (bitsPerSample <= 32)
                                {
                                    sampleSetCallback = RasterSample.SetUInt32DataView;
                                }
                                else if (bitsPerSample <= 64)
                                {
                                    sampleSetCallback = RasterSample.SetUInt64DataView;
                                }
                                else
                                {
                                    throw new InvalidGeoTiffException("Unsupported data format/bitsPerSample");
                                }

                                break;
                            case 2: // twos complement signed integer data
                                if (bitsPerSample <= 8)
                                {
                                    sampleSetCallback = RasterSample.SetInt8DataView;
                                }
                                else if (bitsPerSample <= 16)
                                {
                                    sampleSetCallback = RasterSample.SetInt16DataView;
                                }
                                else if (bitsPerSample <= 32)
                                {
                                    sampleSetCallback = RasterSample.SetInt32DataView;
                                }
                                else if (bitsPerSample <= 64)
                                {
                                    sampleSetCallback = RasterSample.SetInt64DataView;
                                }
                                else
                                {
                                    throw new InvalidGeoTiffException("Unsupported data format/bitsPerSample");
                                }

                                break;
                            case 3:
                                switch (bitsPerSample)
                                {
                                    case 16: 
                                        sampleSetCallback = RasterSample.SetFloat16DataView;
                                        break;
                                    case 32:
                                        sampleSetCallback = RasterSample.SetFloat32DataView;
                                        break;
                                    case 64:
                                        sampleSetCallback = RasterSample.SetFloat64DataView;
                                        break;
                                    default:
                                        throw new InvalidGeoTiffException("Unsupported data format/bitsPerSample");
                                }

                                break;
                            default:
                                throw new InvalidGeoTiffException("Unsupported data format/bitsPerSample");
                        }

                        var currentSample = rasterSamples[si];
                        
                        for (ulong y = startY; y < ymax; ++y)
                        {
                            for (ulong x = startX; x < xmax; ++x)
                            {
                                ulong pixelOffset = ((y * tileWidth) + x) * bytesPerPixelToUse;
                                ulong windowCoordinate = (
                                    (y + firstLine - imageWindow[1]) * windowWidth
                                ) + x + firstCol - imageWindow[0];

                                
                                var dv = dataView;
                                sampleSetCallback(currentSample, dv, (int)pixelOffset + srcSampleOffsets[si], windowCoordinate, _littleEndian);
                            }
                        }

                        return true;
                    });
                    promises.Add(promise);
                }
            }
        }

        await Task.WhenAll(promises);
        return new Raster(rasterSamples, this.GetOrCalculateAffineTransformation(), imageWindowWidth, imageWindowHeight, this, (maxYTile - minYTile) * (maxXTile - minXTile));
    }

    public Raster ReadRaster(
        ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null)
    {
        ulong[] imageWindow = new ulong[] { 0, 0, Width, Height };

        if (window is not null)
        {
            imageWindow = window.ToArray();
        }

        if (imageWindow[0] > imageWindow[2] || imageWindow[1] > imageWindow[3])
        {
            throw new GeoTiffException("Invalid image window");
        }

        ulong imageWindowWidth = imageWindow[2] - imageWindow[0];
        ulong imageWindowHeight = imageWindow[3] - imageWindow[1];

        ulong numPixels = imageWindowWidth * imageWindowHeight;

        IEnumerable<int> samples = Enumerable.Range(0, (int)SamplesPerPixel).ToArray();

        if (sampleSelection is not null)
        {
            samples = sampleSelection.ToArray();
        }

        SparseList<RasterSample> rasterSamples = new();

        for (int i = 0; i < samples.Count(); ++i)
        {
            var sampleIndex = samples.ElementAt(i);
            var sampleDataType = SampleDataTypeForSample(sampleIndex);

            rasterSamples[sampleIndex] = new RasterSample(
                imageWindowWidth,
                imageWindowHeight,
                this,
                sampleDataType,
                (int)numPixels);
        }

        var blockInfo = GetBlockInfo(imageWindow);

        var tileWidth = GetTileOrStripWidth();
        var tileHeight = GetTileOrStripHeight();

        ulong minXTile = blockInfo.MinXTile;
        ulong maxXTile = blockInfo.MaxXTile;
        ulong minYTile = blockInfo.MinYTile;
        ulong maxYTile = blockInfo.MaxYTile;

        var imageWidth = Width;
        var imageHeight = Height;
        var windowWidth = imageWindow[2] - imageWindow[0];

        ulong bytesPerPixel = GetNumberOfBytesPerPixel();

        SparseList<int> srcSampleOffsets = new();

        for (int i = 0; i < samples.Count(); ++i)
        {
            int sample = samples.ElementAt(i);

            if (_planarConfiguration == 1)
            {
                srcSampleOffsets.Add(sample, sum(BitsPerSample, 0, sample) / 8);
            }
            else
            {
                srcSampleOffsets.Add(sample, 0);
            }
        }

        if (!_isTiled)
        {
            GetStripOffsets();
            GetStripByteCounts();
        }
        else
        {
            GetTileOffsets();
            GetTileByteCounts();
        }

        for (ulong yTile = minYTile; yTile < maxYTile; ++yTile)
        {
            for (ulong xTile = minXTile; xTile < maxXTile; ++xTile)
            {
                for (int sampleIndex = 0; sampleIndex < samples.Count(); ++sampleIndex)
                {
                    int si = samples.ElementAt(sampleIndex);

                    TileOrStripResult tile;

                    if (_planarConfiguration == 1)
                    {
                        tile = GetTileOrStrip(xTile, yTile, 0);
                    }
                    else
                    {
                        tile = GetTileOrStrip(xTile, yTile, si);
                    }

                    byte[] buffer = tile.data;
                    var dataView = new DataView(buffer);

                    ulong blockHeight = GetBlockHeight(tile.y);
                    ulong firstLine = tile.y * tileHeight;
                    ulong firstCol = tile.x * tileWidth;

                    ulong lastLine = firstLine + blockHeight;
                    ulong lastCol = (tile.x + 1) * tileWidth;

                    ulong ymax = JSMath.JSMin(
                        blockHeight,
                        blockHeight - (lastLine - imageWindow[3]),
                        imageHeight - firstLine);

                    ulong xmax = JSMath.JSMin(
                        (ulong)tileWidth,
                        (ulong)(tileWidth - (lastCol - imageWindow[2])),
                        (ulong)(imageWidth - firstCol));

                    ulong startY = imageWindow[1] > firstLine
                        ? imageWindow[1] - firstLine
                        : 0;

                    ulong startX = imageWindow[0] > firstCol
                        ? imageWindow[0] - firstCol
                        : 0;

                    for (ulong y = startY; y < ymax; ++y)
                    {
                        for (ulong x = startX; x < xmax; ++x)
                        {
                            ulong bytesPerPixelToUse = _planarConfiguration == 2
                                ? GetSampleByteSize(si)
                                : bytesPerPixel;

                            ulong pixelOffset = ((y * tileWidth) + x) * bytesPerPixelToUse;

                            ulong windowCoordinate =
                                ((y + firstLine - imageWindow[1]) * windowWidth)
                                + x + firstCol - imageWindow[0];

                            ushort format = SampleFormat is not null
                                ? SampleFormat[si]
                                : (ushort)1;

                            ushort bitsPerSample = GetBitsForSample(si);

                            var currentSample = rasterSamples[si];

                            switch (format)
                            {
                                case 1: // unsigned
                                    if (bitsPerSample <= 8)
                                    {
                                        var v = dataView.GetUInt8((int)pixelOffset + srcSampleOffsets[si]);
                                        currentSample.SetUInt8(v, (int)windowCoordinate);
                                    }
                                    else if (bitsPerSample <= 16)
                                    {
                                        var v = dataView.GetUInt16((int)pixelOffset + srcSampleOffsets[si], _littleEndian);
                                        currentSample.SetUInt16(v, (int)windowCoordinate);
                                    }
                                    else if (bitsPerSample <= 32)
                                    {
                                        var v = dataView.GetUInt32((int)pixelOffset + srcSampleOffsets[si], _littleEndian);
                                        currentSample.SetUInt32(v, (int)windowCoordinate);
                                    }
                                    else
                                    {
                                        var v = dataView.GetUInt64((int)pixelOffset + srcSampleOffsets[si], _littleEndian);
                                        currentSample.SetUInt64(v, (int)windowCoordinate);
                                    }
                                    break;

                                case 2: // signed
                                    if (bitsPerSample <= 8)
                                    {
                                        var v = dataView.GetInt8((int)pixelOffset + srcSampleOffsets[si]);
                                        currentSample.SetInt8(v, (int)windowCoordinate);
                                    }
                                    else if (bitsPerSample <= 16)
                                    {
                                        var v = dataView.GetInt16((int)pixelOffset + srcSampleOffsets[si], _littleEndian);
                                        currentSample.SetInt16(v, (int)windowCoordinate);
                                    }
                                    else if (bitsPerSample <= 32)
                                    {
                                        var v = dataView.GetInt32((int)pixelOffset + srcSampleOffsets[si], _littleEndian);
                                        currentSample.SetInt32(v, (int)windowCoordinate);
                                    }
                                    else
                                    {
                                        var v = dataView.GetInt64((int)pixelOffset + srcSampleOffsets[si], _littleEndian);
                                        currentSample.SetInt64(v, (int)windowCoordinate);
                                    }
                                    break;

                                case 3: // float
                                    switch (bitsPerSample)
                                    {
                                        case 16:
                                            currentSample.SetFloat16(
                                                dataView.GetFloat16((int)pixelOffset + srcSampleOffsets[si], _littleEndian),
                                                (int)windowCoordinate);
                                            break;

                                        case 32:
                                            currentSample.SetFloat32(
                                                dataView.GetFloat32((int)pixelOffset + srcSampleOffsets[si], _littleEndian),
                                                (int)windowCoordinate);
                                            break;

                                        case 64:
                                            currentSample.SetFloat64(
                                                dataView.GetFloat64((int)pixelOffset + srcSampleOffsets[si], _littleEndian),
                                                (int)windowCoordinate);
                                            break;

                                        default:
                                            throw new InvalidGeoTiffException("Unsupported data format/bitsPerSample");
                                    }
                                    break;

                                default:
                                    throw new InvalidGeoTiffException("Unsupported data format/bitsPerSample");
                            }
                        }
                    }
                }
            }
        }

        return new Raster(
            rasterSamples,
            GetOrCalculateAffineTransformation(),
            imageWindowWidth,
            imageWindowHeight,
            this,
            (maxYTile - minYTile) * (maxXTile - minXTile));
    }
    
    /// <summary>
    /// Read the raster and consider masking, if the raster is masked. If it is not masked, this is the same as calling ReadRasterAsync
    /// </summary>
    /// <param name="window"></param>
    /// <param name="sampleSelection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<Raster> ReadRasterMaskedAsync(
        ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null)
    {
        if (this._parentFile.IsMasked is false)
        {
            return await ReadRasterAsync(window, sampleSelection, cancellationToken);
        }

        this._parentFile.MaskStrategy.ValidateRasterReadArguments(window, sampleSelection);
        
        var mainReadResult = await ReadRasterAsync(window, sampleSelection, cancellationToken);
        await this._parentFile.MaskStrategy.SetMaskValues(this._parentFile, mainReadResult, window, sampleSelection,
            cancellationToken);
        return mainReadResult;
    }
    
    private GeoTiffBlockInfo GetBlockInfo(ulong[]? imageWindow = null)
    {
        if (imageWindow == null)
        {
            imageWindow = new ulong[] { 0, 0, Width, Height };
        }
        
        var tileWidth = GetTileOrStripWidth();
        var tileHeight = GetTileOrStripHeight();
        var imageWidth = Width;
        var imageHeight = Height;
        ulong minXTile = (ulong)Math.Max(Math.Floor((double)imageWindow[0] / (double)tileWidth), 0);
        ulong maxXTile = (ulong)Math.Min(
            Math.Ceiling((double)imageWindow[2] / (double)tileWidth),
            Math.Ceiling((double)imageWidth / tileWidth)
        );
        ulong minYTile = (ulong)Math.Max(Math.Floor((double)imageWindow[1] / (double)tileHeight), 0);
        ulong maxYTile = (ulong)Math.Min(
            Math.Ceiling((double)imageWindow[3] / (double)tileHeight),
            Math.Ceiling((double)imageHeight / (double)tileHeight)
        );
        return new GeoTiffBlockInfo()
        {
            MinXTile = minXTile, MaxXTile = maxXTile, MinYTile = minYTile, MaxYTile = maxYTile
        };
    }

    /// <summary>
    /// Get all block ImagePixelWindows that cover a bigger ImagePixelWindow
    /// </summary>
    /// <param name="imageWindow"></param>
    /// <returns></returns>
    public IEnumerable<ImagePixelWindow> GetBlockImagePixelWindows(ImagePixelWindow? imageWindow = null)
    {
        if (imageWindow == null)
        {
            imageWindow = ImagePixelWindow.FromArray(new ulong[] { 0, 0, Width, Height });
        }

        var blockInfo = GetBlockInfo(imageWindow.ToArray());

        var tileWidth = GetTileOrStripWidth();
        var tileHeight = GetTileOrStripHeight();

        for (ulong yTile = blockInfo.MinYTile; yTile < blockInfo.MaxYTile; yTile++)
        {
            for (ulong xTile = blockInfo.MinXTile; xTile < blockInfo.MaxXTile; xTile++)
            {
                // Tile bounds in image space
                ulong xMin = xTile * tileWidth;
                ulong yMin = yTile * tileHeight;

                ulong xMax = Math.Min(xMin + tileWidth, Width);
                ulong yMax = Math.Min(yMin + tileHeight, Height);
                var arr = imageWindow.ToArray();
                // Intersect with requested image window
                ulong winXMin = Math.Max(xMin, arr[0]);
                ulong winYMin = Math.Max(yMin, arr[1]);

                ulong winXMax = Math.Min(xMax, arr[2]);
                ulong winYMax = Math.Min(yMax, arr[3]);

                // Skip empty intersections
                if (winXMax <= winXMin || winYMax <= winYMin)
                {
                    continue;
                }

                yield return ImagePixelWindow.FromArray([
                    winXMin,
                    winYMin,
                    winXMax,
                    winYMax
                ]);
            }
        }
    }
    
    private int sum(IEnumerable<ushort> array, int start, int end) {
        var s = 0;
        for (var i = start; i < end; ++i) {
            s += array.ElementAt(i);
        }
        return s;
    }
    
    /// <summary>
    /// Check the sample types before reading them.
    /// </summary>
    /// <param name="sampleIndex"></param>
    /// <returns></returns>
    public GeotiffSampleDataType GetSampleType(int sampleIndex = 0)
    {
        int format = SampleFormat is not null
            ? SampleFormat[sampleIndex]
            : 1;
        
        ushort bitsPerSample = GetBitsForSample(sampleIndex);
        switch (format)
        {
            case 1: // unsigned integer data
                switch (bitsPerSample)
                {
                    case <= 8:
                        return GeotiffSampleDataType.UInt8;
                    case <= 16:
                        return GeotiffSampleDataType.UInt16;
                    case <= 32:
                        return GeotiffSampleDataType.UInt32;
                }

                break;
            case 2: // twos complement signed integer data
                switch (bitsPerSample)
                {
                    case <= 8:
                        return GeotiffSampleDataType.Int8;
                    case <= 16:
                        return GeotiffSampleDataType.Int16;
                    case <= 32:
                        return GeotiffSampleDataType.Int32;
                }

                break;
            case 3:
                switch (bitsPerSample)
                {
                    case 16:
                        return GeotiffSampleDataType.Float16;
                    case 32:
                        return GeotiffSampleDataType.Float32;
                    case 64:
                        return GeotiffSampleDataType.Float64;
                }

                break;
        }

        throw new InvalidGeoTiffException("Unsupported data format/bitsPerSample");
    }
    
    /// <summary>
    /// Returns the decoded strip or tile.
    /// </summary>
    /// <param name="blockX"></param>
    /// <param name="blockY"></param>
    /// <param name="sample"></param>
    /// <param name="poolOrDecoder"></param>
    /// <param name="signal"></param>
    /// <returns></returns>
    private async Task<TileOrStripResult> GetTileOrStripAsync(ulong blockX, ulong blockY, int sample,
        CancellationToken? signal)
    {
        ulong numTilesPerRow = (ulong)Math.Ceiling((double)Width / (double)GetTileOrStripWidth());
        ulong numTilesPerCol = (ulong)Math.Ceiling((double)Height / (double)GetTileOrStripHeight());
        ulong index = 0;
        var sampleToUse = 0;
        if (_planarConfiguration == 1)
        {
            index = (blockY * numTilesPerRow) + blockX;
        }
        else if (_planarConfiguration == 2)
        {
            sampleToUse = sample;
            index = ((ulong)sampleToUse * numTilesPerRow * numTilesPerCol) + (blockY * numTilesPerRow) + blockX;
        }

        ulong offset;
        ulong byteCount;
        if (_isTiled)
        {
            offset = GetTileOffsets().ElementAt((int)index);
            byteCount = GetTileByteCounts().ElementAt((int)index);
        }
        else
        {
            offset = GetStripOffsets().ElementAt((int)index);
            byteCount = GetStripByteCounts().ElementAt((int)index);
        }

        if (byteCount == 0) // for GDAL_SPARSE
        {
            ulong nPixels = GetBlockHeight(blockY) * GetTileOrStripWidth();
            ulong bytesPerPixel = _planarConfiguration == 2
                ? GetSampleByteSize(sampleToUse)
                : GetNumberOfBytesPerPixel();
            
            var data = new byte[nPixels * bytesPerPixel];
            
            var sampleType = GetSampleType();
            var view = new DataView(data, sampleType);
            
            var gdalNoData = GetGdalNoData();
            if (gdalNoData is not null)
            {
                view.FillValue(gdalNoData, sampleType);
            }

            return new TileOrStripResult { x = blockX, y = blockY, data = data};
        }

        byte[] sliceBytes =
            (await _source.FetchAsync(new List<Slice>() { new(offset, byteCount) }, signal)).First();

        Func<Task<byte[]>> request;
        byte[] finalData;
        if (_tileCache == null || _tileCache.ContainsKey(index) is false)
        {
            var predictor = this.GetPredictor();
            // resolve each request by potentially applying array normalization
            request = async () =>
            {
                int sampleFormat = GetSampleFormat();
                uint bitsForCurrentSample = GetBitsForSample(sampleToUse);
                byte[] data = await DecoderRegistry.DecodeAsync(this, sliceBytes, predictor);

                if (NeedsNormalization(sampleFormat, (int)bitsForCurrentSample))
                {
                    if (bitsForCurrentSample == 1)
                    {
                        // The space needed to store your bits, rounded up to the nearest 8 bits.
                        int NearestMultipleCeil(int value, int multiple) =>
                            ((value + multiple - 1) / multiple) * multiple;

                        // Bits are arranged by row. However, they are byte-padded, so e.g. if your image width
                        // is 50, you'll have something like this:
                        //  11111111 11111111 11111111 11111111 11111111 11111111 11000000 row 1
                        //  11111111 11111111 11111111 11111111 11111111 11111111 11000000 row 2 
                        // with 50 valid bits + 6 padding bits for byte alignment

                        var bitsPerRow = NearestMultipleCeil((int)Width, 8);
                        var nRows = data.Length * 8 /
                                    bitsPerRow; // doesn't have to be GetTileOrStripWidth(), could be less if it's an end strip

                        byte[] output = new byte[(int)Width * nRows];

                        int outputIndex = 0;
                        int rowBitIndex = 0;

                        foreach (byte b in data)
                        {
                            for (int n = 7; n >= 0; n--) // MSB → LSB
                            {
                                if (rowBitIndex < (int)Width)
                                {
                                    output[outputIndex++] = (byte)((b >> n) & 1); // get nth bit from a byte
                                }

                                rowBitIndex++;
                                if (rowBitIndex >= bitsPerRow)
                                {
                                    rowBitIndex = 0;
                                }
                            }
                        }

                        return output;
                    }

                    throw new NotSupportedException(
                        $"Only bit data normalization is supported. SampleFormat is {sampleFormat}, bitsForCurrentSample is {(int)bitsForCurrentSample}");
                }

                return data;
            };
            finalData = await request();
            // set the cache
            if (_tileCache != null)
            {
                _tileCache[index] = finalData;
            }
        }
        else
        {
            // get from the cache
            finalData = _tileCache[index];
        }

        // cache the tile request
        return new TileOrStripResult() { x = blockX, y = blockY, data = finalData};
    }
    
    
    private TileOrStripResult GetTileOrStrip(ulong blockX, ulong blockY, int sample)
    {
        ulong numTilesPerRow = (ulong)Math.Ceiling((double)Width / (double)GetTileOrStripWidth());
        ulong numTilesPerCol = (ulong)Math.Ceiling((double)Height / (double)GetTileOrStripHeight());
        ulong index = 0;
        var sampleToUse = 0;
        if (_planarConfiguration == 1)
        {
            index = (blockY * numTilesPerRow) + blockX;
        }
        else if (_planarConfiguration == 2)
        {
            sampleToUse = sample;
            index = ((ulong)sampleToUse * numTilesPerRow * numTilesPerCol) + (blockY * numTilesPerRow) + blockX;
        }

        ulong offset;
        ulong byteCount;
        if (_isTiled)
        {
            offset = GetTileOffsets().ElementAt((int)index);
            byteCount = GetTileByteCounts().ElementAt((int)index);
        }
        else
        {
            offset = GetStripOffsets().ElementAt((int)index);
            byteCount = GetStripByteCounts().ElementAt((int)index);
        }

        if (byteCount == 0) // for GDAL_SPARSE
        {
            ulong nPixels = GetBlockHeight(blockY) * GetTileOrStripWidth();
            ulong bytesPerPixel = _planarConfiguration == 2
                ? GetSampleByteSize(sampleToUse)
                : GetNumberOfBytesPerPixel();
            
            var data = new byte[nPixels * bytesPerPixel];
            
            var sampleType = GetSampleType();
            var view = new DataView(data, sampleType);
            
            var gdalNoData = GetGdalNoData();
            if (gdalNoData is not null)
            {
                view.FillValue(gdalNoData, sampleType);
            }

            return new TileOrStripResult { x = blockX, y = blockY, data = data};
        }

        byte[] sliceBytes = (_source.Fetch(new List<Slice>() { new(offset, byteCount) })).First();

        Func<Task<byte[]>> request;
        byte[] finalData;
        if (_tileCache == null || _tileCache.ContainsKey(index) is false)
        {
            var predictor = this.GetPredictor();
            // resolve each request by potentially applying array normalization

            int sampleFormat = GetSampleFormat();
            uint bitsForCurrentSample = GetBitsForSample(sampleToUse);
            byte[] data = DecoderRegistry.Decode(this, sliceBytes, predictor);

            if (NeedsNormalization(sampleFormat, (int)bitsForCurrentSample))
            {
                if (bitsForCurrentSample == 1)
                {
                    // The space needed to store your bits, rounded up to the nearest 8 bits.
                    int NearestMultipleCeil(int value, int multiple) =>
                        ((value + multiple - 1) / multiple) * multiple;

                    // Bits are arranged by row. However, they are byte-padded, so e.g. if your image width
                    // is 50, you'll have something like this:
                    //  11111111 11111111 11111111 11111111 11111111 11111111 11000000 row 1
                    //  11111111 11111111 11111111 11111111 11111111 11111111 11000000 row 2 
                    // with 50 valid bits + 6 padding bits for byte alignment

                    var bitsPerRow = NearestMultipleCeil((int)Width, 8);
                    var nRows = data.Length * 8 /
                                bitsPerRow; // doesn't have to be GetTileOrStripWidth(), could be less if it's an end strip

                    byte[] output = new byte[(int)Width * nRows];

                    int outputIndex = 0;
                    int rowBitIndex = 0;

                    foreach (byte b in data)
                    {
                        for (int n = 7; n >= 0; n--) // MSB → LSB
                        {
                            if (rowBitIndex < (int)Width)
                            {
                                output[outputIndex++] = (byte)((b >> n) & 1); // get nth bit from a byte
                            }

                            rowBitIndex++;
                            if (rowBitIndex >= bitsPerRow)
                            {
                                rowBitIndex = 0;
                            }
                        }
                    }

                    finalData = output;
                }

                throw new NotSupportedException(
                    $"Only bit data normalization is supported. SampleFormat is {sampleFormat}, bitsForCurrentSample is {(int)bitsForCurrentSample}");
            }
            else
            {
                finalData = data;    
            }

            // set the cache
            if (_tileCache != null)
            {
                _tileCache[index] = finalData;
            }
        }
        else
        {
            // get from the cache
            finalData = _tileCache[index];
        }

        // cache the tile request
        return new TileOrStripResult() { x = blockX, y = blockY, data = finalData};
    }

    private bool NeedsNormalization(int format, int bitsPerSample)
    {
        if ((format == 1 || format == 2) && bitsPerSample <= 64 && bitsPerSample % 8 == 0)
        {
            return false;
        }
        if (format == 3 && (bitsPerSample == 16 || bitsPerSample == 32 || bitsPerSample == 64))
        {
            return false;
        }

        return true;
    }

    public ulong GetBlockWidth()
    {
        return GetTileOrStripWidth();
    }

    private ulong GetBlockHeight(ulong y)
    {
        if (_isTiled || (y + 1) * GetTileOrStripHeight() <= Height)
        {
            return GetTileOrStripHeight();
        }
        else
        {
            return Height - (y * GetTileOrStripHeight());
        }
    }
    
    /// <summary>
    /// Experimental. Returns null if the CRS is not set. 
    /// </summary>
    /// <returns></returns>
    public CoordinateReferenceSystemInfo? GetCoordinateReferenceSystemInfo()
    {
        var crsInfo = new CoordinateReferenceSystemInfo();
        var modelTypeTag = _fileDirectory.GetGeoTag("GTModelTypeGeoKey");
        if (modelTypeTag == null)
        {
            return null;
        }
        crsInfo.ModelType = modelTypeTag.GetUShort();

        if (crsInfo.ModelType == 0)
        {
            return null; // undefined
        }
        
        if (crsInfo.ModelType == 1)//projected CS
        {
            var projectedCSTypeGeoKey = _fileDirectory.GetGeoTag("ProjectedCSTypeGeoKey");
            var projectedCRSGeoKey = _fileDirectory.GetGeoTag("ProjectedCRSGeoKey");
            //GeoTIFF v1.0
            if (projectedCSTypeGeoKey != null)
            {
                crsInfo.ProjectedCRS = projectedCSTypeGeoKey.GetUShort();
            }
            //GeoTIFF v1.1
            else if (projectedCRSGeoKey != null)
            {
                crsInfo.ProjectedCRS = projectedCRSGeoKey.GetUShort();
            }
        }
        else if (crsInfo.ModelType is 2 or 3)//geographic CS
        {
            var geographicTypeGeoKey = _fileDirectory.GetGeoTag("GeographicTypeGeoKey");
            var geodeticCRSGeoKey = _fileDirectory.GetGeoTag("GeodeticCRSGeoKey");
            var geogGeodeticCRSGeoKey = _fileDirectory.GetGeoTag("GeogGeodeticDatumGeoKey");
            if (geographicTypeGeoKey != null) //GeoTIFF v1.0
            {
                crsInfo.GeographicCRS = geographicTypeGeoKey.GetUShort();   
            }
            else if (geodeticCRSGeoKey != null) //GeoTIFF v1.1
            {
                crsInfo.GeographicCRS = geodeticCRSGeoKey.GetUShort();
            }
            else if (geogGeodeticCRSGeoKey != null)
            {
                crsInfo.GeographicCRS = geogGeodeticCRSGeoKey.GetUShort();
            }
            else
            {
                throw new GeoTiffException("Unrecognised geographic or geocentric coordinate system");
            }
                
        }
        else if (crsInfo.ModelType == 32767)
        {
            return crsInfo;
        }
        else
        {
            throw new GeoTiffException("Unsupported CRS model type");
        }

        var verticalCSTypeGeoKey = _fileDirectory.GetGeoTag("VerticalCSTypeGeoKey");
        var verticalGeoKey = _fileDirectory.GetGeoTag("VerticalGeoKey");
        if (verticalCSTypeGeoKey != null) //GeoTIFF v1.0
        {
            crsInfo.VerticalModelCRS = verticalCSTypeGeoKey.GetUShort();
        }
        else if (verticalGeoKey != null) //GeoTIFF v1.1
        {
            crsInfo.VerticalModelCRS = verticalGeoKey.GetUShort();
        }
        
        return crsInfo;
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
        // If the user passed a low x, we want to be close to the X origin.
        double left = pixelOrigin.X;
        double right = left + 1; 

        // if the user passed a low y, be far away from the Y origin.

        double top = pixelOrigin.Y;
        double bottom = top + 1; 

        var window = new ImagePixelWindow()
        {
            MinColumn = (ulong)left, 
            MaxColumn = (ulong)right, 
            MinRow = (ulong)top, 
            MaxRow = (ulong)bottom
        };

        return await ReadRasterAsync(window, sampleSelection, cancellationToken);
    }
    
    /// <summary>
    /// Use the affine transformation to transform a bounding box in model space to pixel space.
    /// This assumes that your bounding box is in the same CRS as the dataset.
    /// Returns null if the affine transformation is not set. 
    /// </summary>
    /// <returns></returns>
    public ImagePixelWindow? BoundingBoxToPixelWindow(BoundingBox bbox)
    {
        var affine = this.GetOrCalculateAffineTransformation();

        if (affine is null)
        {
            return null;
        }

        var bottomLeft = affine.ModelToPixel(bbox.XMin, bbox.YMin);
        var topRight = affine.ModelToPixel(bbox.XMax, bbox.YMax);

        // Affine operation will wrap around; prevent this from being used.
        if (bottomLeft.X < 0 || bottomLeft.Y < 0 || topRight.X < 0 || topRight.Y < 0)
        {
            throw new GeoTiffException("Specified bounding box extends beyond bounding box of image");
        }
        
        return new ImagePixelWindow()
        {
            MinColumn = (ulong)bottomLeft.X, 
            MaxColumn = (ulong)topRight.X, 
            MinRow = (ulong)topRight.Y, 
            MaxRow = (ulong)bottomLeft.Y 
        };
    }
}