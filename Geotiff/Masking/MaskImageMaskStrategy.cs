namespace Geotiff.Masking;

public class MaskImageMaskStrategy:MaskStrategyABC
{
    private readonly int MaskImageIndex;
    public readonly byte YesDataValue;
    public MaskImageMaskStrategy(int maskImageIndex, byte yesDataValue)
    {
        this.MaskImageIndex = maskImageIndex;
        this.YesDataValue = yesDataValue;

    }
    public override async Task SetMaskValues(GeoTiffReader parentFile,Raster mainReadResult, ImagePixelWindow? window = null,
        IEnumerable<int>? sampleSelection = null,
        CancellationToken? cancellationToken = null)
    {
        var maskImage = await parentFile.GetImageAsync(this.MaskImageIndex);
        var maskRead = await maskImage.ReadRasterAsync(window, sampleSelection, cancellationToken);
        var maskSample = maskRead.GetSampleAt(0);
        var byteArray = maskSample.GetByteArray();
        
        for (int i = 0; i < byteArray.Length; i++)
        {
            foreach (var sample in mainReadResult.GetAllReadSamples())
            {
                sample.SetMaskedAtIndex(i,byteArray[i] != YesDataValue);
            }
        }
    }
}