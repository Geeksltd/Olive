using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;

namespace Olive.Aws.Rekognition
{
    public static class TextDetector
    {
        private static IAmazonRekognition _client;
        static string BucketName => Config.Get<string>("AWS:Rekognition:S3Bucket");

        public static IAmazonRekognition Client => _client ?? Context.Current.GetOptionalService<IAmazonRekognition>() ?? new AmazonRekognitionClient(RegionEndpoint.EUWest1);

        /// <summary>
        ///  Creates and uses a new Aws Client in the specified region.
        /// </summary>
        public static void Region(Amazon.RegionEndpoint region) => SetClient(new AmazonRekognitionClient(region));
        private static void SetClient(IAmazonRekognition client)
        {
            _client = client;
        }

        /// <summary>
        ///  gets the image based on the location in the s3 bucket configured AWS:Rekognition:S3Bucket.
        /// </summary>
        public static async Task<List<TextDetection>> GetTextDetetionResult(string photo)
        {
            if (BucketName == null)
            {
                throw new KeyNotFoundException("AWS:Rekognition:S3Bucket missing from your configuration");
            }
            return await GetTextDetetionResult(photo, BucketName);

        }

        /// <summary>
        ///  gets the image based on the location in a specific Bucket.
        /// </summary>
        public static async Task<List<TextDetection>> GetTextDetetionResult(string photo, string bucketName)
        {
            DetectTextRequest detectTextRequest = new DetectTextRequest()
            {
                Image = new Image()
                {
                    S3Object = new S3Object()
                    {
                        Name = photo,
                        Bucket = bucketName
                    }
                }
            };
            try
            {
                DetectTextResponse detectTextResponse = await Client.DetectTextAsync(detectTextRequest);
                return detectTextResponse.TextDetections;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        ///  Gets the image using a memorystream.
        /// </summary>
        public static async Task<List<TextDetection>> GetTextDetetionResult(MemoryStream photo)
        {
            DetectTextRequest detectTextRequest = new DetectTextRequest(){Image = new Image(){Bytes= photo}};

            try
            {
                DetectTextResponse detectTextResponse = await Client.DetectTextAsync(detectTextRequest);
                return detectTextResponse.TextDetections;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
    }
}
