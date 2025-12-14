using Geotiff.Exceptions;

namespace Geotiff;

/// <summary>
/// TODO: add static method to do this from single dataset GeoTIFF with internal mask band
/// </summary>
public class MaskedGeoTIFFFilesystemReader
{
    private readonly MultiGeoTIFF multiGeoTiff;
    private MaskedGeoTIFFFilesystemReader(MultiGeoTIFF multiGeoTiff)
    {
        this.multiGeoTiff = multiGeoTiff;
    }
    
    public static async Task<MaskedGeoTIFFFilesystemReader> FromMultiGeoTiff(MultiGeoTIFF multiGeoTiff)
    {
        var count = await multiGeoTiff.GetImageCount();
        if (count < 2)
        {
            throw new InvalidMaskedGeoTIFFException("Masked MultiGeoTIFFs require at least 2 images");
        }

        var mainImage = await multiGeoTiff.GetImage();
        var maskImage = await multiGeoTiff.GetImage(1);

        if (mainImage.GetHeight() != maskImage.GetHeight() || mainImage.GetWidth() != maskImage.GetWidth())
        {
            throw new InvalidMaskedGeoTIFFException("Mask file must have the same dimensions as the main file");
        }
            
        
        
        return new MaskedGeoTIFFFilesystemReader(multiGeoTiff);
    }
    
    
}