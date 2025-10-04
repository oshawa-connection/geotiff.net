using Geotiff.JavaScriptCompatibility;

namespace Geotiff.RemoteClients;
// No filepath: user should decide where to place this code
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;


public class GeotiffAWSClient :IGeotiffRemoteClient
{
    private string bucketName { get; set; }
    private string key{ get; set; }
    private AmazonS3Client amazonS3Client { get; set; }
    public GeotiffAWSClient(string bucketName, string key, AmazonS3Client amazonS3Client)
    {
        this.bucketName = bucketName;
        this.key = key;
        this.amazonS3Client = amazonS3Client;
    }
    private async Task<ArrayBuffer> DownloadRangeFromS3Async(long start, long end, CancellationToken? signal = null)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            ByteRange = new ByteRange(start, end) // inclusive range,
        };

        using var response = await this.amazonS3Client.GetObjectAsync(request);
        await using var stream = response.ResponseStream;
        return await ArrayBuffer.FromStream(stream, signal);
    }

    public async Task<IEnumerable<ArrayBuffer>> FetchSlices(IEnumerable<Slice> slices, CancellationToken? signal = null)
    {
        var tasks = slices.Select(slice => DownloadRangeFromS3Async(slice.offset, slice.length + slice.offset, signal));
        return await Task.WhenAll(tasks);
    }

    public async Task<ArrayBuffer> FetchSlice(Slice slice, CancellationToken? signal = null)
    {
        return await this.DownloadRangeFromS3Async(slice.offset, slice.length + slice.offset, signal);
    }
}