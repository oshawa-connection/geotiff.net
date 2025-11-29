using Amazon.S3;
using Amazon.S3.Model;
using Geotiff;
using Geotiff.RemoteClients;

namespace RemoteTests;

[TestClass]
public class S3Tests
{
    [TestMethod]
    public async Task Example()
    {
        using var client =
            new AmazonS3Client(new AmazonS3Config { ServiceURL = "http://127.0.0.1:8085", ForcePathStyle = true });
        ListObjectsV2Response? response =
            await client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = "testbucket" });
        foreach (S3Object? obj in response.S3Objects)
        {
            Console.WriteLine(obj.Key);
        }

        string? key = response.S3Objects[0].Key;
        GetObjectResponse? getObjectResponse = await client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = "testbucket", Key = key
        });


        using (var reader = new StreamReader(getObjectResponse.ResponseStream))
        {
            string? fileContents = await reader.ReadToEndAsync();
        }
    }

    [TestMethod]
    public async Task ReadGeotiffFromBucket()
    {
        using var client =
            new AmazonS3Client(new AmazonS3Config { ServiceURL = "http://127.0.0.1:8085", ForcePathStyle = true });
        var gtAWSClient = new GeotiffAWSClient("testbucket", "cea.tif", client);
        GeoTIFF? geotiff = await GeoTIFF.FromRemoteClient(gtAWSClient);
        Console.WriteLine(geotiff.GetImageCount());
    }
}