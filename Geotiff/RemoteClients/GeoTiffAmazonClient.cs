using Geotiff.Extensions;
using Geotiff.JavaScriptCompatibility;

namespace Geotiff.RemoteClients;

// No filepath: user should decide where to place this code
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;

public class GeoTiffAmazonClient : IGeoTiffRemoteClient
{
    private string bucketName { get; set; }
    private string key { get; set; }
    private AmazonS3Client amazonS3Client { get; set; }

    public GeoTiffAmazonClient(string bucketName, string key, AmazonS3Client amazonS3Client)
    {
        this.bucketName = bucketName;
        this.key = key;
        this.amazonS3Client = amazonS3Client;
    }

    private async Task<byte[]> DownloadRangeFromS3Async(long start, long end, CancellationToken? signal = null)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName, Key = key, ByteRange = new ByteRange(start, end) // inclusive range,
        };

        using GetObjectResponse? response = await amazonS3Client.GetObjectAsync(request);
        await using Stream? stream = response.ResponseStream;
        return await stream.ToByteArray(signal);
    }
    
    public async Task<IEnumerable<byte[]>> FetchSlicesAsync(IEnumerable<Slice> slices, CancellationToken? signal = null)
    {
        IEnumerable<Task<byte[]>>? tasks = slices.Select(slice =>
            DownloadRangeFromS3Async((long)slice.Offset, (long)(slice.Length + slice.Offset), signal));
        return await Task.WhenAll(tasks);
    }

    public async Task<byte[]> FetchSliceAsync(Slice slice, CancellationToken? signal = null)
    {
        return await DownloadRangeFromS3Async((long)slice.Offset, (long)(slice.Length + slice.Offset), signal);
    }

    public IEnumerable<byte[]> FetchSlices(IEnumerable<Slice> slices)
    {
        IEnumerable<Task<byte[]>>? tasks = slices.Select(slice =>
            DownloadRangeFromS3Async((long)slice.Offset, (long)(slice.Length + slice.Offset)));
        var task = Task.WhenAll(tasks);
        task.Wait();
        return task.Result;
    }

    public byte[] FetchSlice(Slice slice)
    {
        var task = Task.Run(() => FetchSliceAsync(slice)); 
        task.Wait();
        return task.Result;
    }
}