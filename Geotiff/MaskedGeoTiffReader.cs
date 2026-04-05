using Geotiff.Exceptions;

namespace Geotiff;

/// <summary>
/// TODO: Hardcode for now, but need to make this extensible.
/// </summary>
public enum MaskedGeoTiffStrategy
{
    EXTERNAL_MSK_FILE,
    INTERNAL_ALPHA_BAND,
    NO_DATA_VALUE
}


/// <summary>
/// There are three cases that need to be handled here:
/// 1. External .msk file.
/// 2. Internal alpha band mask
/// 3. 1-X bands with the GDAL_NODATA tag
///
/// There is also potentially a 4th case
/// "Specify a per-band NODATA value as part of a suggested encodingInfo extension to the RangeType DataRecord fields (which also addresses the scale factor and offset)"
/// But I haven't seen it used.
/// </summary>
/// This does not inherit from GeoTiff; they are almost totally different.
public class MaskedGeoTiffReader
{
    private MaskedGeoTiffStrategy _strategy;
    private readonly MultiGeoTiff multiGeoTiff;
    private MaskedGeoTiffReader(MultiGeoTiff multiGeoTiff)
    {
        this.multiGeoTiff = multiGeoTiff;
        this._strategy = MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE;
    }
    
    /// <summary>
    /// This is the case 1 where the tiff has to maintain backwards compatability i.e. have 1-3 bands
    /// for clients that only support that, so they write the mask band out to a seperate .msk file.
    /// </summary>
    /// <param name="multiGeoTiff"></param>
    /// <returns></returns>
    /// <exception cref="InvalidMaskedGeoTiffException"></exception>
    public static async Task<MaskedGeoTiffReader> FromMultiGeoTiff(MultiGeoTiff multiGeoTiff)
    {
        var count = await multiGeoTiff.GetImageCountAsync();
        if (count < 2)
        {
            throw new InvalidMaskedGeoTiffException("Masked MultiGeoTIFFs require at least 2 images");
        }

        var mainImage = await multiGeoTiff.GetImageAsync();
        var maskImage = await multiGeoTiff.GetImageAsync(1);
        // TODO: check type of mask band here - should be byte. Double check this from GDAL.
        
        if (mainImage.Height != maskImage.Height || mainImage.Width != maskImage.Width)
        {
            throw new InvalidMaskedGeoTiffException("Mask file must have the same dimensions as the main file");
        }
        
        return new MaskedGeoTiffReader(multiGeoTiff);
    }

    
    
}