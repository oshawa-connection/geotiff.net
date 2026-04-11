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
/// It is up to the user to determine which of these situations is present. 
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
    public const byte INTERNAL_MASK_YES_DATA_VALUE = 1;
    public const byte EXTERNAL_MASK_YES_DATA_VALUE = 255;
    // This breaks the open closed principle. Temporary implementation detail, can be re-worked later.
    private MaskedGeoTiffStrategy _strategy;
    private readonly GeoTiff multiGeoTiff;
    private MaskedGeoTiffReader(GeoTiff multiGeoTiff, MaskedGeoTiffStrategy strat)
    {
        this.multiGeoTiff = multiGeoTiff;
        this._strategy = strat;
    }
    
    public static async Task<MaskedGeoTiffReader> FromInternalMaskGeoTiffAsync(GeoTiff internalMaskBandTiff)
    {
        if (await internalMaskBandTiff.GetImageCountAsync() < 2)
        {
            throw new InvalidMaskedGeoTiffException("There must be at least two samples for a masked tif with an internal mask.");
        }
        
        var mainImage = await internalMaskBandTiff.GetImageAsync(0);
        var maskImage = await internalMaskBandTiff.GetImageAsync(1);

        if (maskImage.GetNumberOfSamples() != 1)
        {
            throw new InvalidMaskedGeoTiffException("Unrecognised internal mask; mask should have only one sample");
        }
        
        var maskSampleType = maskImage.GetSampleType(0);
        
        if (maskSampleType != GeotiffSampleDataType.UInt8)
        {
            throw new InvalidMaskedGeoTiffException("Masked raster mask image sample should be an unsigned byte type");
        }
        
        var multi = new MultiGeoTiff(internalMaskBandTiff,new List<GeoTiff>());
        
        return new MaskedGeoTiffReader(multi, MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE);
    }
    
    /// <summary>
    /// This is the case 1 where the tiff has to maintain backwards compatability i.e. have 1-3 bands
    /// for clients that only support that, so they write the mask band out to a seperate .msk file,
    /// or for cases where the tiff is readonly so they aren't free to add new bands.
    /// </summary>
    /// <param name="multiGeoTiff"></param>
    /// <returns></returns>
    /// <exception cref="InvalidMaskedGeoTiffException"></exception>
    public static async Task<MaskedGeoTiffReader> FromExternalMaskGeoTiffAsync(MultiGeoTiff multiGeoTiff)
    {
        var count = await multiGeoTiff.GetImageCountAsync();
        if (count < 2)
        {
            throw new InvalidMaskedGeoTiffException("Masked MultiGeoTIFFs require at least 2 images");
        }
        
        // TODO: Handle multiple images
        var mainImage = await multiGeoTiff.GetImageAsync();
        var maskImage = await multiGeoTiff.GetImageAsync(1);
        
        // There are a number of checks we could do here - however, we want to keep this incredibly flexible for future.
        // E.g. don't check the number of samples; it should be 1 but if there are more just ignore them.

        var maskSampleType = maskImage.GetSampleType(0);
        if (maskSampleType != GeotiffSampleDataType.UInt8)
        {
            throw new InvalidMaskedGeoTiffException("Masked raster mask image sample should be an unsigned byte type");
        }
        
        if (mainImage.Height != maskImage.Height || mainImage.Width != maskImage.Width)
        {
            throw new InvalidMaskedGeoTiffException("Mask file must have the same dimensions as the main file");
        }
        
        return new MaskedGeoTiffReader(multiGeoTiff, MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE);
    }

    // public async Task<MaskedRaster> ReadMaskedRasterBoundingBoxAsync(BoundingBox boundingBox,
    //     IEnumerable<int>? sampleSelection = null, CancellationToken? cancellationToken = null)
    // {
    //     var mainImage = await multiGeoTiff.GetImageAsync();
    //     var maskImage = await multiGeoTiff.GetImageAsync(1);
    //
    //     var mainImageReadResult = await mainImage.ReadRasterBoundingBoxAsync(boundingBox, sampleSelection, cancellationToken);
    //     // Mask image doesn't always contain affine
    //     var pixelWindow = mainImage.BoundingBoxToPixelWindow(boundingBox);
    //     var maskImageReadResult = await maskImage.ReadRasterAsync(pixelWindow, sampleSelection, cancellationToken);
    //     return new MaskedRaster(mainImageReadResult, maskImageReadResult, null, 0, 0, this);
    // }


    public async Task<MaskedRaster> ReadMaskedRasterAsync(ImagePixelWindow? window = null, IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null)
    {
        if (this._strategy == MaskedGeoTiffStrategy.EXTERNAL_MSK_FILE)
        {
            var mainImage = await multiGeoTiff.GetImageAsync();
            var maskImage = await multiGeoTiff.GetImageAsync(1);

            var mainImageReadResult = await mainImage.ReadRasterAsync(window, sampleSelection, cancellationToken);
            var maskImageReadResult = await maskImage.ReadRasterAsync(window, sampleSelection, cancellationToken);
            return new MaskedRaster(mainImageReadResult, maskImageReadResult, null, 0, 0, this);
        }

        if (this._strategy == MaskedGeoTiffStrategy.INTERNAL_ALPHA_BAND)
        {
            var mainImage = await multiGeoTiff.GetImageAsync();
            var allsamples = await mainImage.ReadRasterAsync(window, sampleSelection, cancellationToken);
            return new MaskedRaster(allsamples);
        }

        throw new GeoTiffException("Exception occurred during reading of masked geotiff");
    }
}