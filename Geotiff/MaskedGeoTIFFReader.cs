using Geotiff.Exceptions;

namespace Geotiff;

/// <summary>
/// There are three cases that need to be handled here:
/// 1. External .msk file.
/// 2. Internal alpha band mask
/// 3. 1-X bands with the GDAL_NODATA tag
///
/// There is also potentially a 4th case
/// "Specify a per-band NODATA value as part of a suggested encodingInfo extension to the RangeType DataRecord fields (which also addresses the scale factor and offset)"
/// But I haven't seen it used.
///
/// 
/// TODO: add static method to do this from single dataset GeoTIFF with internal mask band
/// </summary>
public class MaskedGeoTIFFReader
{
    private readonly MultiGeoTIFF multiGeoTiff;
    private MaskedGeoTIFFReader(MultiGeoTIFF multiGeoTiff)
    {
        this.multiGeoTiff = multiGeoTiff;
    }
    
    /// <summary>
    /// This is the case 1 where the tiff has to maintain backwards compatability i.e. have 1-3 bands
    /// for clients that only support that, so they write the mask band out to a seperate .msk file.
    /// </summary>
    /// <param name="multiGeoTiff"></param>
    /// <returns></returns>
    /// <exception cref="InvalidMaskedGeoTiffException"></exception>
    public static async Task<MaskedGeoTIFFReader> FromMultiGeoTiff(MultiGeoTIFF multiGeoTiff)
    {
        var count = await multiGeoTiff.GetImageCountAsync();
        if (count < 2)
        {
            throw new InvalidMaskedGeoTiffException("Masked MultiGeoTIFFs require at least 2 images");
        }

        var mainImage = await multiGeoTiff.GetImageAsync();
        var maskImage = await multiGeoTiff.GetImageAsync(1);
        // TODO: check type of mask band here - should be byte. Double check this from GDAL.
        
        if (mainImage.GetHeight() != maskImage.GetHeight() || mainImage.GetWidth() != maskImage.GetWidth())
        {
            throw new InvalidMaskedGeoTiffException("Mask file must have the same dimensions as the main file");
        }
        
        return new MaskedGeoTIFFReader(multiGeoTiff);
    }

    public async Task<MaskBandGeoTIFFReadResult<T>> ReadMaskedRasters<T>(ImageWindow? window = null, CancellationToken? cancellationToken = null) where T : struct
    {
        var mainImage = await multiGeoTiff.GetImageAsync();
        var maskImage = await multiGeoTiff.GetImageAsync(1);

        var mainReadResult = await mainImage.ReadRastersAsync<T>(window, cancellationToken);
        var maskedReadResult = await maskImage.ReadRastersAsync<byte>(window, cancellationToken);
        
        return new MaskBandGeoTIFFReadResult<T>(
            maskedReadResult.GetSampleResultAt(0).FlatData, 
            mainReadResult.SampleData, 
            maskedReadResult.Width, 
            maskedReadResult.Height, 
            mainImage);
    }
}