using Geotiff.JavaScriptCompatibility;

namespace Geotiff.RemoteClients;
// No filepath: user should decide where to place this code
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
using System.Threading.Tasks;


public class GeotiffAWSClient
{
    public async Task DownloadRangeFromS3Async(string bucketName, string key, long start, long end)
    {
        using var client = new AmazonS3Client();
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            ByteRange = new ByteRange(start, end) // inclusive range,
            
        };

        using var response = await client.GetObjectAsync(request);
        using var stream = response.ResponseStream;
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        byte[] data = ms.ToArray();
        // new ArrayBuffer()
    }
}