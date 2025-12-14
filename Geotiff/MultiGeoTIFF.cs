using Geotiff.Exceptions;
using System.Collections.Generic;

namespace Geotiff;

/// <summary>
/// This is taken from geotiff.js implementation, however, it can also be used for other sidecar files.
/// </summary>
public class MultiGeoTIFF : GeoTIFF
{
    private readonly GeoTIFF mainFile;
    private readonly IEnumerable<GeoTIFF> sidecarFileSources;
    private IEnumerable<int>? imageCounts;
    public MultiGeoTIFF(GeoTIFF mainFile, IEnumerable<GeoTIFF> sidecarFileSources,  bool isLittleEndian, bool isBigTiff, ulong firstIFDOffset) : base(mainFile.Source, isLittleEndian,isBigTiff,firstIFDOffset)
    {
        this.mainFile = mainFile;
        this.sidecarFileSources = sidecarFileSources;
    }

    public static async Task<MultiGeoTIFF> FromStreams(Stream mainStream, IEnumerable<Stream> otherStreams)
    {
        var mainTiff = await GeoTIFF.FromStream(mainStream);
        var x = otherStreams.Select(async d => await GeoTIFF.FromStream(d));
        var r = await Task.WhenAll(x);

        return new MultiGeoTIFF(mainTiff, r, mainTiff.IsLittleEndian, mainTiff.IsBifTIFF, mainTiff.FirstIFDOffset);
    }
    
    private async Task ParseFileDirectoriesForAllFiles()
    {
        var tasks2 = this.sidecarFileSources.Select(d => d.ParseFileDirectoryAt((int)d.FirstIFDOffset));
        var tasks3 = tasks2.Append(this.mainFile.ParseFileDirectoryAt((int)this.mainFile.FirstIFDOffset));

        await Task.WhenAll(tasks3);
    }

    public override async Task<GeoTiffImage> GetImage(int index = 0) {
        await this.GetImageCount();
        await this.ParseFileDirectoriesForAllFiles();
        var visited = 0;
        var relativeIndex = 0;
        var imageFiles = new List<GeoTIFF>() {this.mainFile};
        imageFiles.AddRange(this.sidecarFileSources);
        for (var i = 0; i < imageFiles.Count; i++) {
            var imageFile = imageFiles[i];
            for (var ii = 0; ii < this.imageCounts.ElementAt(i); ii++) {
                if (index == visited) {
                    var ifd = await imageFile.RequestIFD(relativeIndex);
                    return new GeoTiffImage(
                        ifd, this.IsLittleEndian, false,
                        imageFile.Source
                    );
                }
                visited++;
                relativeIndex++;
            }
            relativeIndex = 0;
        }
    
        throw new GeoTiffImageIndexError(index);
    }
    
    
    public override async Task<int> GetImageCount()
    {
        if (this.finalImageCount != null) 
        {
            return (int)this.finalImageCount;
        }

        var ics = new List<Task<int>>()
        {
            this.mainFile.GetImageCount()
        };
        
        ics.AddRange(this.sidecarFileSources.Select(d => d.GetImageCount()));
        this.imageCounts = await Task.WhenAll(ics);
        this.finalImageCount = this.imageCounts.Sum();
        return (int)this.finalImageCount;
    }
}