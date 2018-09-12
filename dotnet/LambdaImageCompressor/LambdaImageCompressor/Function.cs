using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using TinyPng;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaImageCompressor
{
    public class Function
    {
        private readonly string[] _supportedImagetypes = new string[] { ".png", ".jpg"};
        private readonly AmazonS3Client _s3Client;

        public Function()
        {
            _s3Client = new AmazonS3Client();
        }

        /// <summary>
        /// A simple function that takes pictures from S3 and compress them
        /// </summary>
        /// <param name="s3Event"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// 
        // through  NullReferenceException ?
        public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
        {
            foreach(var record in s3Event.Records)
            {
                if (!_supportedImagetypes.Contains(Path.GetExtension(record.S3.Object.Key).ToLower()))
                {
                    Console.WriteLine($"Object {record.S3.Bucket.Name}:{record.S3.Object.Key} is not a supported image type");
                    continue;
                }

                Console.WriteLine($"Determining whether image {record.S3.Bucket.Name}:{record.S3.Object.Key} has been compressed");

                // Get the existing tag set
                var taggingResponse = await _s3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
                {
                    BucketName = record.S3.Bucket.Name,
                    Key = record.S3.Object.Key
                });

                if (taggingResponse.Tagging.Any(tag => tag.Key == "Compressed" && tag.Value == "true"))
                {
                    Console.WriteLine($"Image {record.S3.Bucket.Name}:{record.S3.Object.Key} has been compressed");
                    continue;
                }

                // Get the existing image
                using (var objectResponse = await _s3Client.GetObjectAsync(record.S3.Bucket.Name, record.S3.Object.Key))
                using (Stream responseStream = objectResponse.ResponseStream)
                {
                    Console.WriteLine($"Compressing image {record.S3.Bucket.Name}:{record.S3.Object.Key}");

                    // Use TinyPNG to compress the image
                    //TinyPngClient tinyPngClient = new TinyPngClient(Environment.GetEnvironmentVariable("TinyPNG_API_Key"));
                    //var compressResponse = await tinyPngClient.Compress(responseStream);
                    //var downloadResponse = await tinyPngClient.SaveCompressedImageToAmazonS3(compressResponse, record.S3.Bucket.Name);

                    // Upload the compress image back to S3
                    Console.WriteLine($"Uploading compressed image {record.S3.Bucket.Name}:{record.S3.Object.Key}");

                    await _s3Client.PutObjectAsync(new PutObjectRequest
                    {
                        BucketName = record.S3.Bucket.Name,
                        Key = record.S3.Object.Key,
                        InputStream = responseStream,
                        TagSet = new List<Tag>
                        {
                            new Tag
                            {
                                Key = "Compressed",
                                Value = "true"
                            }
                        }
                    });
                }
            }
        }
    }
}
