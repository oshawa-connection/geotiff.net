namespace Geotiff.Masking;

public class AlphaBandMaskStrategy :MaskStrategyABC
{
    public override async Task SetMaskValues(GeoTiff parentFile, Raster mainReadResult, ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null)
    {
        var maskSample = mainReadResult.GetSampleAt(3);
        var alphaArray = maskSample.GetByteArray();

        var r = mainReadResult.GetSampleAt(0);
        var g = mainReadResult.GetSampleAt(1);
        var b = mainReadResult.GetSampleAt(2);
        
        for (int i = 0; i < alphaArray.Length; i++)
        {
            if (alphaArray[i] == 0)
            {
                r.SetMaskedAtIndex(i);
                g.SetMaskedAtIndex(i);
                b.SetMaskedAtIndex(i);
            }
        }
    }

    public override void ValidateRasterReadArguments(ImagePixelWindow? window = null, IEnumerable<int>? sampleSelection = null)
    {
        if (sampleSelection?.Contains(3) is false)
        {
            throw new ArgumentException("When using alpha band masking, you must read the alpha band to consider masking");
        }
        base.ValidateRasterReadArguments(window, sampleSelection);
    }
}