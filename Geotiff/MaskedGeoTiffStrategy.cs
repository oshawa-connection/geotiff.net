namespace Geotiff;

// /// <summary>
// /// TODO: Hardcode for now, but need to make this extensible.
// /// </summary>
public enum MaskedGeoTiffStrategy
{
    IS_NOT_MASKED,
    EXTERNAL_MSK_FILE,
    INTERNAL_MASK,
    NO_DATA_VALUE,
    ALPHA_BAND
}