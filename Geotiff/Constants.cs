namespace Geotiff;

public static class Constants
{
    public const UInt16 BOMLittleEndian = 0x4949;
    public const UInt16 BOMBigEndian = 0x4D4D;
    public const UInt16 LittleTifMagicValue = 42;
    public const UInt16 BigTifMagicValue = 43;
    public const int WGS84_EPSG_CODE = 4326; 
}


internal static class FieldTypes
{
    public const string BYTE = "BYTE";
    public const string ASCII = "ASCII";
    public const string SHORT = "SHORT";
    public const string LONG = "LONG";
    public const string RATIONAL = "RATIONAL";
    public const string SBYTE = "SBYTE";
    public const string UNDEFINED = "UNDEFINED";
    public const string SSHORT = "SSHORT";
    public const string SLONG = "SLONG";
    public const string SRATIONAL = "SRATIONAL";
    public const string FLOAT = "FLOAT";
    public const string DOUBLE = "DOUBLE";
    public const string IFD = "IFD";
    public const string LONG8 = "LONG8";
    public const string SLONG8 = "SLONG8";
    public const string IFD8 = "IFD8";
    
    public static Dictionary<int, string> FieldTypeLookup = new Dictionary<int, string>()
    {
        { 0x0001, FieldTypes.BYTE },
        { 0x0002, FieldTypes.ASCII },
        { 0x0003, FieldTypes.SHORT },
        { 0x0004, FieldTypes.LONG },
        { 0x0005, FieldTypes.RATIONAL },
        { 0x0006, FieldTypes.SBYTE },
        { 0x0007, FieldTypes.UNDEFINED },
        { 0x0008, FieldTypes.SSHORT },
        { 0x0009, FieldTypes.SLONG },
        { 0x000A, FieldTypes.SRATIONAL },
        { 0x000B, FieldTypes.FLOAT },
        { 0x000C, FieldTypes.DOUBLE },
        { 0x000D, FieldTypes.IFD },
        { 0x0010, FieldTypes.LONG8 },
        { 0x0011, FieldTypes.SLONG8 },
        { 0x0012, FieldTypes.IFD8 }
    };


    public static string BitsPerSample = "BitsPerSample";
    public static string ExtraSamples = "ExtraSamples";
    public static string SampleFormat = "SampleFormat";
    public static string StripByteCounts = "StripByteCounts";
    public static string StripOffsets = "StripOffsets";
    public static string StripRowCounts = "StripRowCounts";
    public static string TileByteCounts = "TileByteCounts";
    public static string TileOffsets = "TileOffsets";
    public static string SubIFDs = "SubIFDs";
    public static string ModelTiepoint = "ModelTiepoint";
    public static string ModelPixelScale = "ModelPixelScale";
    public static string ModelTransformation = "ModelTransformation";
    public static string ImageWidth = "ImageWidth";
    public static string ImageLength = "ImageLength";
    public static string SamplesPerPixel = "SamplesPerPixel";
    public static string TileWidth = "TileWidth";
    public static string TileLength = "TileLength";
    public static string RowsPerStrip = "RowsPerStrip";
    public static string PlanarConfiguration = "PlanarConfiguration";
    
    public static BiDirectionalDictionary<ushort, string> FieldTags = new BiDirectionalDictionary<ushort, string>()
    {
        // TIFF Baseline
        { 0x013B, "Artist" },
        { 0x0102, BitsPerSample },
        { 0x0109, "CellLength" },
        { 0x0108, "CellWidth" },
        { 0x0140, "ColorMap" },
        { 0x0103, "Compression" },
        { 0x8298, "Copyright" },
        { 0x0132, "DateTime" },
        { 0x0152, ExtraSamples },
        { 0x010A, "FillOrder" },
        { 0x0121, "FreeByteCounts" },
        { 0x0120, "FreeOffsets" },
        { 0x0123, "GrayResponseCurve" },
        { 0x0122, "GrayResponseUnit" },
        { 0x013C, "HostComputer" },
        { 0x010E, "ImageDescription" },
        { 0x0101, ImageLength },
        { 0x0100, ImageWidth },
        { 0x010F, "Make" },
        { 0x0119, "MaxSampleValue" },
        { 0x0118, "MinSampleValue" },
        { 0x0110, "Model" },
        { 0x00FE, "NewSubfileType" },
        { 0x0112, "Orientation" },
        { 0x0106, "PhotometricInterpretation" },
        { 0x011C, PlanarConfiguration },
        { 0x0128, "ResolutionUnit" },
        { 0x0116, RowsPerStrip },
        { 0x0115, SamplesPerPixel },
        { 0x0131, "Software" },
        { 0x0117, StripByteCounts },
        { 0x0111, StripOffsets },
        { 0x00FF, "SubfileType" },
        { 0x0107, "Threshholding" },
        { 0x011A, "XResolution" },
        { 0x011B, "YResolution" },

        // TIFF Extended
        { 0x0146, "BadFaxLines" },
        { 0x0147, "CleanFaxData" },
        { 0x0157, "ClipPath" },
        { 0x0148, "ConsecutiveBadFaxLines" },
        { 0x01B1, "Decode" },
        { 0x01B2, "DefaultImageColor" },
        { 0x010D, "DocumentName" },
        { 0x0150, "DotRange" },
        { 0x0141, "HalftoneHints" },
        { 0x015A, "Indexed" },
        { 0x015B, "JPEGTables" },
        { 0x011D, "PageName" },
        { 0x0129, "PageNumber" },
        { 0x013D, "Predictor" },
        { 0x013F, "PrimaryChromaticities" },
        { 0x0214, "ReferenceBlackWhite" },
        { 0x0153, SampleFormat },
        { 0x0154, "SMinSampleValue" },
        { 0x0155, "SMaxSampleValue" },
        { 0x022F, StripRowCounts },
        { 0x014A, SubIFDs },
        { 0x0124, "T4Options" },
        { 0x0125, "T6Options" },
        { 0x0145, TileByteCounts },
        { 0x0143, TileLength },
        { 0x0144, TileOffsets },
        { 0x0142,  TileWidth},
        { 0x012D, "TransferFunction" },
        { 0x013E, "WhitePoint" },
        { 0x0158, "XClipPathUnits" },
        { 0x011E, "XPosition" },
        { 0x0211, "YCbCrCoefficients" },
        { 0x0213, "YCbCrPositioning" },
        { 0x0212, "YCbCrSubSampling" },
        { 0x0159, "YClipPathUnits" },
        { 0x011F, "YPosition" },

        // EXIF
        { 0x9202, "ApertureValue" },
        { 0xA001, "ColorSpace" },
        { 0x9004, "DateTimeDigitized" },
        { 0x9003, "DateTimeOriginal" },
        { 0x8769, "Exif IFD" },
        { 0x9000, "ExifVersion" },
        { 0x829A, "ExposureTime" },
        { 0xA300, "FileSource" },
        { 0x9209, "Flash" },
        { 0xA000, "FlashpixVersion" },
        { 0x829D, "FNumber" },
        { 0xA420, "ImageUniqueID" },
        { 0x9208, "LightSource" },
        { 0x927C, "MakerNote" },
        { 0x9201, "ShutterSpeedValue" },
        { 0x9286, "UserComment" },

        // IPTC
        { 0x83BB, "IPTC" },

        // Laser Scanning Microscopy
        { 0x866C, "CZ_LSMINFO" },

        // ICC
        { 0x8773, "ICC Profile" },

        // XMP
        { 0x02BC, "XMP" },

        // GDAL
        { 0xA480, "GDAL_METADATA" },
        { 0xA481, "GDAL_NODATA" },

        // Photoshop
        { 0x8649, "Photoshop" },

        // GeoTiff
        { 0x830E, ModelPixelScale },
        { 0x8482, ModelTiepoint },
        { 0x85D8, ModelTransformation },
        { 0x87AF, "GeoKeyDirectory" },
        { 0x87B0, "GeoDoubleParams" },
        { 0x87B1, "GeoAsciiParams" },

        // LERC
        { 0xC5F2, "LercParameters" },
    };

    public static List<ushort> ArrayTypeFields = new List<ushort>()
    {
        FieldTags.GetByValue(BitsPerSample),
        FieldTags.GetByValue(ExtraSamples),
        FieldTags.GetByValue(SampleFormat),
        FieldTags.GetByValue(StripByteCounts),
        FieldTags.GetByValue(StripOffsets),
        FieldTags.GetByValue(StripRowCounts),
        FieldTags.GetByValue(TileByteCounts),
        FieldTags.GetByValue(TileOffsets),
        FieldTags.GetByValue(SubIFDs),
    };
    
    
    public static BiDirectionalDictionary<ushort, string> GeoKeyNames = new BiDirectionalDictionary<ushort, string>
    {
        { 1024, "GTModelTypeGeoKey" },
        { 1025, "GTRasterTypeGeoKey" },
        { 1026, "GTCitationGeoKey" },
        { 2048, "GeographicTypeGeoKey" },
        { 2049, "GeogCitationGeoKey" },
        { 2050, "GeogGeodeticDatumGeoKey" },
        { 2051, "GeogPrimeMeridianGeoKey" },
        { 2052, "GeogLinearUnitsGeoKey" },
        { 2053, "GeogLinearUnitSizeGeoKey" },
        { 2054, "GeogAngularUnitsGeoKey" },
        { 2055, "GeogAngularUnitSizeGeoKey" },
        { 2056, "GeogEllipsoidGeoKey" },
        { 2057, "GeogSemiMajorAxisGeoKey" },
        { 2058, "GeogSemiMinorAxisGeoKey" },
        { 2059, "GeogInvFlatteningGeoKey" },
        { 2060, "GeogAzimuthUnitsGeoKey" },
        { 2061, "GeogPrimeMeridianLongGeoKey" },
        { 2062, "GeogTOWGS84GeoKey" },
        { 3072, "ProjectedCSTypeGeoKey" },
        { 3073, "PCSCitationGeoKey" },
        { 3074, "ProjectionGeoKey" },
        { 3075, "ProjCoordTransGeoKey" },
        { 3076, "ProjLinearUnitsGeoKey" },
        { 3077, "ProjLinearUnitSizeGeoKey" },
        { 3078, "ProjStdParallel1GeoKey" },
        { 3079, "ProjStdParallel2GeoKey" },
        { 3080, "ProjNatOriginLongGeoKey" },
        { 3081, "ProjNatOriginLatGeoKey" },
        { 3082, "ProjFalseEastingGeoKey" },
        { 3083, "ProjFalseNorthingGeoKey" },
        { 3084, "ProjFalseOriginLongGeoKey" },
        { 3085, "ProjFalseOriginLatGeoKey" },
        { 3086, "ProjFalseOriginEastingGeoKey" },
        { 3087, "ProjFalseOriginNorthingGeoKey" },
        { 3088, "ProjCenterLongGeoKey" },
        { 3089, "ProjCenterLatGeoKey" },
        { 3090, "ProjCenterEastingGeoKey" },
        { 3091, "ProjCenterNorthingGeoKey" },
        { 3092, "ProjScaleAtNatOriginGeoKey" },
        { 3093, "ProjScaleAtCenterGeoKey" },
        { 3094, "ProjAzimuthAngleGeoKey" },
        { 3095, "ProjStraightVertPoleLongGeoKey" },
        { 3096, "ProjRectifiedGridAngleGeoKey" },
        { 4096, "VerticalCSTypeGeoKey" },
        { 4097, "VerticalCitationGeoKey" },
        { 4098, "VerticalDatumGeoKey" },
        { 4099, "VerticalUnitsGeoKey" },
    };
    
    public static int GetFieldTypeLength(int fieldTypea)
    {
        var fieldType = FieldTypeLookup[fieldTypea];
        switch (fieldType) {
            case FieldTypes.BYTE: case FieldTypes.ASCII: case FieldTypes.SBYTE: case FieldTypes.UNDEFINED:
                return 1;
            case FieldTypes.SHORT: case FieldTypes.SSHORT:
                return 2;
            case FieldTypes.LONG: case FieldTypes.SLONG: case FieldTypes.FLOAT: case FieldTypes.IFD:
                return 4;
            case FieldTypes.RATIONAL: case FieldTypes.SRATIONAL: case FieldTypes.DOUBLE:
            case FieldTypes.LONG8: case FieldTypes.SLONG8: case FieldTypes.IFD8:
                return 8;
            default:
                throw new Exception($"Invalid field type: {fieldType}");
        }
    }
}
