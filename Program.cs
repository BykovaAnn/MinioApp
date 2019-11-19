using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace MinioApp
{
    class Program
    {
        private static string accessKey = "123";
        private static string secretKey = "12345678";
        private static string serviceURL = "http://192.168.99.100:9000";
        private static AmazonS3Client localclient;
        private static readonly RegionEndpoint serverRegion = RegionEndpoint.EUWest1;
        static void Main(string[] args)
        {
            localclient = CreateClient();
            Task.Run(GetAllObjectsAsync).GetAwaiter().GetResult();
            string bucketName = "bucket1";
            string keyName = "key1";
            string content = "somecontent";
            Task.Run(() => WriteObjectAsync(bucketName, keyName, content)).GetAwaiter().GetResult();
            keyName = "key2";
            string filePath = @"D:\Hello12.txt";
            string contentType = "text/plain";
            string title = "Title1";
            Task.Run(() => WriteFileAsync(bucketName, keyName, filePath, contentType, title)).GetAwaiter().GetResult();
            filePath = @"D:\Hello1.txt";
            Task.Run(() => GetFileAsync(bucketName, keyName, filePath)).GetAwaiter().GetResult();
        }

        private static AmazonS3Client CreateClient()
        {
            AmazonS3Config config = new AmazonS3Config
            {
                RegionEndpoint = serverRegion,
                ServiceURL = serviceURL,
                ForcePathStyle = true
            };
            return new AmazonS3Client(accessKey, secretKey, config);
        }
        private static async Task GetAllObjectsAsync()
        {
            ListBucketsResponse listBucketResponse = await localclient.ListBucketsAsync();

            foreach (S3Bucket bucket in listBucketResponse.Buckets)
            {
                Console.Out.WriteLine("Bucket '" + bucket.BucketName + "' created at " + bucket.CreationDate);
            }
            if (listBucketResponse.Buckets.Count > 0)
            {
                string bucketName = listBucketResponse.Buckets[0].BucketName;
                ListObjectsResponse listObjectsResponse = await localclient.ListObjectsAsync(bucketName);
                foreach (S3Object obj in listObjectsResponse.S3Objects)
                {
                    Console.Out.WriteLine("key = '" + obj.Key + "'\t | size = " + obj.Size + "\t | tags = '" + obj.ETag + "'\t | modified = " + obj.LastModified);
                }
            }
        }

        private static async Task WriteObjectAsync(string bucketName, string keyName, string content)
        {
            try
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    ContentBody = content
                };
                PutObjectResponse response = await localclient.PutObjectAsync(putRequest);
                Console.WriteLine("Object put: " + response.HttpStatusCode);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        private static async Task WriteFileAsync(string bucketName, string keyName, string filePath, string contentType, string title)
        {
            try
            {
                PutObjectRequest putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    putRequest.InputStream = stream;
                    PutObjectResponse response = await localclient.PutObjectAsync(putRequest);
                }
                Console.WriteLine("File {0} put", filePath);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        private static async Task GetFileAsync(string bucketName, string keyName, string filePath)
        {
            try
            {
                GetObjectResponse response = await localclient.GetObjectAsync(bucketName, keyName);
                response.ResponseStream.CopyTo(Console.OpenStandardOutput());
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }
    }
}
