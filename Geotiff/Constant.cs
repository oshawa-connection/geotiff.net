using Geotiff.Exceptions;

namespace Geotiff;

public static class Constant
{
    public const ushort BOMLittleEndian = 0x4949;
    public const ushort BOMBigEndian = 0x4D4D;
    public const ushort LittleTifMagicValue = 42;
    public const ushort BigTifMagicValue = 43;
    public const int WGS84_EPSG_CODE = 4326;
}

public enum GeoTiffSampleDataType
{
    Uint8,
    Int8,
    Int16,
    UInt16,
    UInt32,
    UInt64,
    Int32,
    Float32,
    Double
}


internal enum GeoTiffFieldDataType
{
    BYTE,
    ASCII,
    SHORT,
    LONG,
    RATIONAL,
    SBYTE,
    UNDEFINED,
    SSHORT,
    SLONG,
    SRATIONAL,
    FLOAT,
    DOUBLE,
    IFD,
    LONG8,
    SLONG8,
    IFD8
}

public enum TagDataType
{
    BYTE,
    ASCII,
    SHORT,
    LONG,
    RATIONAL,
    SBYTE,
    UNDEFINED,
    SSHORT,
    SLONG,
    SRATIONAL,
    FLOAT,
    DOUBLE,
    IFD,
    LONG8,
    SLONG8,
    IFD8,
    BYTE_ARRAY,
    SHORT_ARRAY,
    LONG_ARRAY,
    RATIONAL_ARRAY,
    SBYTE_ARRAY,
    SSHORT_ARRAY,
    SLONG_ARRAY,
    SRATIONAL_ARRAY,
    FLOAT_ARRAY,
    DOUBLE_ARRAY,
    IFD_ARRAY,
    LONG8_ARRAY,
    SLONG8_ARRAY,
    IFD8_ARRAY
}


public static class FieldTypes
{
    internal static Dictionary<int, GeoTiffFieldDataType> FieldTypeLookup = new()
    {
        { 0x0001, GeoTiffFieldDataType.BYTE },
        { 0x0002, GeoTiffFieldDataType.ASCII },
        { 0x0003, GeoTiffFieldDataType.SHORT },
        { 0x0004, GeoTiffFieldDataType.LONG },
        { 0x0005, GeoTiffFieldDataType.RATIONAL },
        { 0x0006, GeoTiffFieldDataType.SBYTE },
        { 0x0007, GeoTiffFieldDataType.UNDEFINED },
        { 0x0008, GeoTiffFieldDataType.SSHORT },
        { 0x0009, GeoTiffFieldDataType.SLONG },
        { 0x000A, GeoTiffFieldDataType.SRATIONAL },
        { 0x000B, GeoTiffFieldDataType.FLOAT },
        { 0x000C, GeoTiffFieldDataType.DOUBLE },
        { 0x000D, GeoTiffFieldDataType.IFD },
        { 0x0010, GeoTiffFieldDataType.LONG8 },
        { 0x0011, GeoTiffFieldDataType.SLONG8 },
        { 0x0012, GeoTiffFieldDataType.IFD8 }
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

    public static string GeoKeyDirectory = "GeoKeyDirectory";
    public static string MinSampleValue = "MinSampleValue";
    public static string MaxSampleValue = "MaxSampleValue";
    public static string FreeOffsets = "FreeOffsets";
    public static string FreeByteCounts = "FreeByteCounts";
    public static string GrayResponseCurve = "GrayResponseCurve";
    public static string PageNumber = "PageNumber";
    public static string TransferFunction = "TransferFunction";
    public static string WhitePoint = "WhitePoint";
    public static string PrimaryChromacities = "PrimaryChromacities";
    public static string ColorMap = "ColorMap";
    public static string HalftoneHints = "HalftoneHints";
    public static string DotRange = "DotRange";
    public static string SMinSampleValue = "SMinSampleValue";
    public static string SMaxSampleValue = "SMaxSampleValue";
    public static string TransferRange = "TransferRange";
    public static string ClipPath = "ClipPath";
    public static string JPEG_tables = "JPEG tables";
    public static string JPEGLosslessPredictors = "JPEGLosslessPredictors";
    public static string JPEGPointTransforms = "JPEGPointTransforms";
    public static string JPEGQTables = "JPEGQTables";
    public static string JPEGDCTables = "JPEGDCTables";
    public static string JPEGACTables = "JPEGACTables";
    public static string YCbCrCoefficients = "YCbCrCoefficients";
    public static string YCbCrSubsampling = "YCbCrSubsampling";
    public static string ReferenceBlackWhite = "ReferenceBlackWhite";
    public static string XMP = "XMP";
    public static string Matteing = "Matteing";
    public static string DataType = "DataType";
    public static string ImageDepth = "ImageDepth";
    public static string TileDepth = "TileDepth";
    public static string CFARepeatPatternDim = "CFARepeatPatternDim";
    public static string Fnumber = "Fnumber";
    public static string IPTC_NAA = "IPTC";
    public static string ModelPixelScaleTag = "ModelPixelScale";
    public static string IntergraphMatrixTag = "IntergraphMatrix";
    public static string ModelTiepointTag = "ModelTiepoint";
    public static string ColorTable = "ColorTable";
    public static string PixelInensityRange = "PixelInensityRange";
    public static string ImageLayer = "ImageLayer";
    public static string GeoDoubleParamsTag = "GeoDoubleParams";
    public static string TimeZoneOffset = "TimeZoneOffset";
    public static string FaxRecvParams = "FaxRecvParams";
    public static string FaxSubAddress = "FaxSubAddress";
    public static string FaxRecvTime = "FaxRecvTime";
    public static string BrightnessValue = "BrightnessValue";
    public static string ExposureBiasValue = "ExposureBiasValue";
    public static string SubjectDistance = "SubjectDistance";
    public static string FocalLength = "FocalLength";
    public static string FlashEnergy = "FlashEnergy";
    public static string Noise = "Noise";
    public static string ExposureIndex = "ExposureIndex";
    public static string TIFF_EPStandardID = "TIFF/EPStandardID";
    public static string StoNits = "StoNits";
    public static string ImageSourceData = "ImageSourceData";
    public static string PhotoshopAnnotations = "PhotoshopAnnotations";
    public static string DNGVersion = "DNGVersion";
    public static string DNGBackwardVersion = "DNGBackwardVersion";
    public static string LocalizedCameraModel = "LocalizedCameraModel";
    public static string CFAPlaneColor = "CFAPlaneColor";
    public static string LinearizationTable = "LinearizationTable";
    public static string BlackLevelRepeatDim = "BlackLevelRepeatDim";
    public static string BlackLevel = "BlackLevel";
    public static string BlackLevelDeltaH = "BlackLevelDeltaH";
    public static string BlackLevelDeltaV = "BlackLevelDeltaV";
    public static string WhiteLevel = "WhiteLevel";
    public static string DefaultScale = "DefaultScale";
    public static string DefaultCropOrigin = "DefaultCropOrigin";
    public static string DefaultCropSize = "DefaultCropSize";
    public static string ColorMatrix1 = "ColorMatrix1";
    public static string ColorMatrix2 = "ColorMatrix2";
    public static string CameraCalibration1 = "CameraCalibration1";
    public static string CameraCalibration2 = "CameraCalibration2";
    public static string ReductionMatrix1 = "ReductionMatrix1";
    public static string ReductionMatrix2 = "ReductionMatrix2";
    public static string AnalogBalnace = "AnalogBalnace";
    public static string AsShortNeutral = "AsShortNeutral";
    public static string AsShortWhiteXY = "AsShortWhiteXY";
    public static string LensInfo = "LensInfo";
    public static string DNGPrivateDatea = "DNGPrivateDatea";
    public static string GPSVersionID = "GPSVersionID";
    public static string GPSLatitude = "GPSLatitude";
    public static string GPSLongitude = "GPSLongitude";
    public static string GPSTimeStamp = "GPSTimeStamp";
    public static string GPSDestLatitude = "GPSDestLatitude";
    public static string GPSDestLongitude = "GPSDestLongitude";
    public static string GPSProcessingMethod = "GPSProcessingMethod";
    public static string GPSAreaInformation = "GPSAreaInformation";
    public static string ExifVersion = "ExifVersion";
    public static string ISOSpeedRatings = "ISOSpeedRatings";
    public static string OECF = "OECF";
    public static string ComponentsConfiguration = "ComponentsConfiguration";
    public static string SubjectArea = "SubjectArea";
    public static string MakerNote = "MakerNote";
    public static string UserComment = "UserComment";
    public static string FlashpixVersion = "FlashpixVersion";
    public static string SpatialFrequencyResponse = "SpatialFrequencyResponse";
    public static string SubjectLocation = "SubjectLocation";
    public static string CFAPattern = "CFAPattern";
    public static string DeviceSettingDescription = "DeviceSettingDescription";
    public static string JPEGTables = "JPEGTables";

    public static BiDirectionalDictionary<ushort, string> FieldTags = new()
    {
        // TIFF Baseline
        { 0x013B, "Artist" },
        { 0x0102, BitsPerSample },
        { 0x0109, "CellLength" },
        { 0x0108, "CellWidth" },
        { 0x0140, ColorMap },
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
        { 0x015B, JPEGTables },
        { 0x011D, "PageName" },
        { 0x0129, PageNumber },
        { 0x013D, "Predictor" },
        { 0x013F, PrimaryChromacities },
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
        { 0x0142, TileWidth },
        { 0x012D, "TransferFunction" },
        { 0x013E, "WhitePoint" },
        { 0x0158, "XClipPathUnits" },
        { 0x011E, "XPosition" },
        { 0x0211, "YCbCrCoefficients" },
        { 0x0213, "YCbCrPositioning" },
        { 0x0212, YCbCrSubsampling },
        { 0x0159, "YClipPathUnits" },
        { 0x011F, "YPosition" },

        // EXIF
        { 0x9202, "ApertureValue" },
        { 0xA001, "ColorSpace" },
        { 0x9004, "DateTimeDigitized" },
        { 0x9003, "DateTimeOriginal" },
        { 0x8769, "Exif IFD" },
        { 0x9000, ExifVersion },
        { 0x829A, "ExposureTime" },
        { 0xA300, "FileSource" },
        { 0x9209, "Flash" },
        { 0xA000, FlashpixVersion },
        { 0x829D, Fnumber },
        { 0xA420, "ImageUniqueID" },
        { 0x9208, "LightSource" },
        { 0x927C, MakerNote },
        { 0x9201, "ShutterSpeedValue" },
        { 0x9286, UserComment },

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
        { 0x830E, ModelPixelScaleTag },
        { 0x8482, ModelTiepoint },
        { 0x85D8, ModelTransformation },
        { 0x87AF, GeoKeyDirectory },
        { 0x87B0, GeoDoubleParamsTag },
        { 0x87B1, "GeoAsciiParams" },

        // LERC
        { 0xC5F2, "LercParameters" },
        // Added because they are array fields
        { 0x0156, TransferRange },
        { 0x01B5, JPEG_tables },
        { 0x0205, JPEGLosslessPredictors },
        { 0x0206, JPEGPointTransforms },
        { 0x0207, JPEGQTables },
        { 0x0208, JPEGDCTables },
        { 0x0209, JPEGACTables },
        { 0x80E3, Matteing },
        { 0x80E4, DataType },
        { 0x80E5, ImageDepth },
        { 0x80E6, TileDepth },
        { 0x828D, CFARepeatPatternDim },
        { 0x828E, CFAPattern },
        { 0x8480, IntergraphMatrixTag },
        { 0x84E6, ColorTable },
        { 0x84EB, PixelInensityRange },
        { 0x87AC, ImageLayer },
        { 0x8827, ISOSpeedRatings },
        { 0x8828, OECF },
        { 0x882A, TimeZoneOffset },
        { 0x885C, FaxRecvParams },
        { 0x885D, FaxSubAddress },
        { 0x885E, FaxRecvTime },
        { 0x9203, BrightnessValue },
        { 0x9204, ExposureBiasValue },
        { 0x9206, SubjectDistance },
        { 0x920A, FocalLength },
        { 0x920B, FlashEnergy },
        { 0x920C, SpatialFrequencyResponse },
        { 0x920D, Noise },
        { 0x9215, ExposureIndex },
        { 0x9216, TIFF_EPStandardID },
        { 0x923F, StoNits },
        { 0x935C, ImageSourceData },
        { 0xC44F, PhotoshopAnnotations },
        { 0xC612, DNGVersion },
        { 0xC613, DNGBackwardVersion },
        { 0xC615, LocalizedCameraModel },
        { 0xC616, CFAPlaneColor },
        { 0xC618, LinearizationTable },
        { 0xC619, BlackLevelRepeatDim },
        { 0xC61A, BlackLevel },
        { 0xC61B, BlackLevelDeltaH },
        { 0xC61C, BlackLevelDeltaV },
        { 0xC61D, WhiteLevel },
        { 0xC61E, DefaultScale },
        { 0xC61F, DefaultCropOrigin },
        { 0xC620, DefaultCropSize },
        { 0xC621, ColorMatrix1 },
        { 0xC622, ColorMatrix2 },
        { 0xC623, CameraCalibration1 },
        { 0xC624, CameraCalibration2 },
        { 0xC625, ReductionMatrix1 },
        { 0xC626, ReductionMatrix2 },
        { 0xC627, AnalogBalnace },
        { 0xC628, AsShortNeutral },
        { 0xC629, AsShortWhiteXY },
        { 0xC630, LensInfo },
        { 0xC634, DNGPrivateDatea },
        { 0x0000, GPSVersionID },
        { 0x0002, GPSLatitude },
        { 0x0004, GPSLongitude },
        { 0x0007, GPSTimeStamp },
        { 0x0014, GPSDestLatitude },
        { 0x0016, GPSDestLongitude },
        { 0x001B, GPSProcessingMethod },
        { 0x001C, GPSAreaInformation },
        { 0x9101, ComponentsConfiguration },
        { 0x9214, SubjectArea },
        { 0xA214, SubjectLocation },
        { 0xA40B, DeviceSettingDescription }
    };

    public static List<ushort> ArrayTypeFields = new()
    {
        FieldTags.GetByValue(BitsPerSample),
        FieldTags.GetByValue(StripOffsets),
        FieldTags.GetByValue(StripByteCounts),
        FieldTags.GetByValue(MinSampleValue),
        FieldTags.GetByValue(MaxSampleValue),
        FieldTags.GetByValue(FreeOffsets),
        FieldTags.GetByValue(FreeByteCounts),
        FieldTags.GetByValue(GrayResponseCurve),
        FieldTags.GetByValue(PageNumber),
        FieldTags.GetByValue(TransferFunction),
        FieldTags.GetByValue(WhitePoint),
        FieldTags.GetByValue(PrimaryChromacities),
        FieldTags.GetByValue(ColorMap),
        FieldTags.GetByValue(HalftoneHints),
        FieldTags.GetByValue(TileOffsets),
        FieldTags.GetByValue(TileByteCounts),
        FieldTags.GetByValue(SubIFDs),
        FieldTags.GetByValue(DotRange),
        FieldTags.GetByValue(ExtraSamples),
        FieldTags.GetByValue(SampleFormat),
        FieldTags.GetByValue(SMinSampleValue),
        FieldTags.GetByValue(SMaxSampleValue),
        FieldTags.GetByValue(TransferRange),
        FieldTags.GetByValue(ClipPath),
        FieldTags.GetByValue(JPEG_tables),
        FieldTags.GetByValue(JPEGLosslessPredictors),
        FieldTags.GetByValue(JPEGPointTransforms),
        FieldTags.GetByValue(JPEGQTables),
        FieldTags.GetByValue(JPEGDCTables),
        FieldTags.GetByValue(JPEGACTables),
        FieldTags.GetByValue(YCbCrCoefficients),
        FieldTags.GetByValue(YCbCrSubsampling),
        FieldTags.GetByValue(ReferenceBlackWhite),
        FieldTags.GetByValue(XMP),
        FieldTags.GetByValue(Matteing),
        FieldTags.GetByValue(DataType),
        FieldTags.GetByValue(ImageDepth),
        FieldTags.GetByValue(TileDepth),
        FieldTags.GetByValue(CFARepeatPatternDim),
        FieldTags.GetByValue(CFAPattern),
        FieldTags.GetByValue(Fnumber),
        FieldTags.GetByValue(IPTC_NAA),
        FieldTags.GetByValue(ModelPixelScaleTag),
        FieldTags.GetByValue(IntergraphMatrixTag),
        FieldTags.GetByValue(ModelTiepointTag),
        FieldTags.GetByValue(ColorTable),
        FieldTags.GetByValue(PixelInensityRange),
        FieldTags.GetByValue(ImageLayer),
        FieldTags.GetByValue(GeoDoubleParamsTag),
        FieldTags.GetByValue(ISOSpeedRatings),
        FieldTags.GetByValue(OECF),
        FieldTags.GetByValue(TimeZoneOffset),
        FieldTags.GetByValue(FaxRecvParams),
        FieldTags.GetByValue(FaxSubAddress),
        FieldTags.GetByValue(FaxRecvTime),
        FieldTags.GetByValue(BrightnessValue),
        FieldTags.GetByValue(ExposureBiasValue),
        FieldTags.GetByValue(SubjectDistance),
        FieldTags.GetByValue(FocalLength),
        FieldTags.GetByValue(FlashEnergy),
        FieldTags.GetByValue(Noise),
        FieldTags.GetByValue(SubjectLocation),
        FieldTags.GetByValue(ExposureIndex),
        FieldTags.GetByValue(TIFF_EPStandardID),
        FieldTags.GetByValue(StoNits),
        FieldTags.GetByValue(ImageSourceData),
        FieldTags.GetByValue(PhotoshopAnnotations),
        FieldTags.GetByValue(DNGVersion),
        FieldTags.GetByValue(DNGBackwardVersion),
        FieldTags.GetByValue(LocalizedCameraModel),
        FieldTags.GetByValue(CFAPlaneColor),
        FieldTags.GetByValue(LinearizationTable),
        FieldTags.GetByValue(BlackLevelRepeatDim),
        FieldTags.GetByValue(BlackLevel),
        FieldTags.GetByValue(BlackLevelDeltaH),
        FieldTags.GetByValue(BlackLevelDeltaV),
        FieldTags.GetByValue(WhiteLevel),
        FieldTags.GetByValue(DefaultScale),
        FieldTags.GetByValue(DefaultCropOrigin),
        FieldTags.GetByValue(DefaultCropSize),
        FieldTags.GetByValue(ColorMatrix1),
        FieldTags.GetByValue(ColorMatrix2),
        FieldTags.GetByValue(CameraCalibration1),
        FieldTags.GetByValue(CameraCalibration2),
        FieldTags.GetByValue(ReductionMatrix1),
        FieldTags.GetByValue(ReductionMatrix2),
        FieldTags.GetByValue(AnalogBalnace),
        FieldTags.GetByValue(AsShortNeutral),
        FieldTags.GetByValue(AsShortWhiteXY),
        FieldTags.GetByValue(LensInfo),
        FieldTags.GetByValue(DNGPrivateDatea),
        FieldTags.GetByValue(GPSVersionID),
        FieldTags.GetByValue(GPSLatitude),
        FieldTags.GetByValue(GPSLongitude),
        FieldTags.GetByValue(GPSTimeStamp),
        FieldTags.GetByValue(GPSDestLatitude),
        FieldTags.GetByValue(GPSDestLongitude),
        FieldTags.GetByValue(GPSProcessingMethod),
        FieldTags.GetByValue(GPSAreaInformation),
        FieldTags.GetByValue(ExifVersion),
        FieldTags.GetByValue(ISOSpeedRatings),
        FieldTags.GetByValue(OECF),
        FieldTags.GetByValue(ComponentsConfiguration),
        FieldTags.GetByValue(SubjectArea),
        FieldTags.GetByValue(MakerNote),
        FieldTags.GetByValue(UserComment),
        FieldTags.GetByValue(FlashpixVersion),
        FieldTags.GetByValue(SubjectLocation),
        FieldTags.GetByValue(CFAPattern),
        FieldTags.GetByValue(DeviceSettingDescription),
        FieldTags.GetByValue(GeoKeyDirectory),
        FieldTags.GetByValue(ModelTransformation),
        FieldTags.GetByValue(JPEGTables)
    };


    public static BiDirectionalDictionary<ushort, string> GeoKeyNames = new()
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
        { 4099, "VerticalUnitsGeoKey" }
    };

    public static int GetFieldTypeLength(int fieldTypea)
    {
        GeoTiffFieldDataType fieldType = FieldTypeLookup[fieldTypea];
        switch (fieldType)
        {
            case GeoTiffFieldDataType.BYTE:
            case GeoTiffFieldDataType.ASCII:
            case GeoTiffFieldDataType.SBYTE:
            case GeoTiffFieldDataType.UNDEFINED:
                return 1;
            case GeoTiffFieldDataType.SHORT:
            case GeoTiffFieldDataType.SSHORT:
                return 2;
            case GeoTiffFieldDataType.LONG:
            case GeoTiffFieldDataType.SLONG:
            case GeoTiffFieldDataType.FLOAT:
            case GeoTiffFieldDataType.IFD:
                return 4;
            case GeoTiffFieldDataType.RATIONAL:
            case GeoTiffFieldDataType.SRATIONAL:
            case GeoTiffFieldDataType.DOUBLE:
            case GeoTiffFieldDataType.LONG8:
            case GeoTiffFieldDataType.SLONG8:
            case GeoTiffFieldDataType.IFD8:
                return 8;
            default:
                throw new GeoTiffException($"Invalid field type: {fieldType}");
        }
    }
}