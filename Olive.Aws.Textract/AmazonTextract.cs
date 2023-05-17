using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Textract;
using Amazon.Textract.Model;

namespace Olive.Aws.Rekognition
{
    public static class AmazonTextract
    {
        private static IAmazonTextract _client;
        static string BucketName => Config.Get<string>("AWS:Textract:S3Bucket");
        private static string _region = Config.Get<string>("AWS:Textract:Region");


        public static IAmazonTextract Client => _client ?? Context.Current.GetOptionalService<IAmazonTextract>() ?? new AmazonTextractClient(_region == null ? RegionEndpoint.EUWest1 : RegionEndpoint.GetBySystemName(_region));

        /// <summary>
        ///  Creates and uses a new Aws Client in the specified region.
        /// </summary>
        public static void Region(Amazon.RegionEndpoint region) => SetClient(new AmazonTextractClient(region));
        private static void SetClient(IAmazonTextract client)
        {
            _client = client;
        }

        /// <summary>
        ///  gets the document based on the location in the s3 bucket configured AWS:Textract:S3Bucket.
        ///  And extract the text string from it.
        /// </summary>
        public static async Task<string> ExtractTextString(string documentKey)
        {
            if (BucketName == null)
            {
                throw new KeyNotFoundException("AWS:Textract:S3Bucket missing from your configuration");
            }
            return await ExtractTextBlocks(documentKey, BucketName).Select(x=>x.Text).ToString("\n");

        }
        /// <summary>
        ///  gets the document based on the location in a specific Bucket.
        ///  And extract the text string from it.
        /// </summary>
        public static async Task<string> ExtractTextString(string documentKey, string bucketName)
        {

            return await ExtractTextBlocks(documentKey, bucketName).Select(x => x.Text).ToString("\n");

        }
        /// <summary>
        ///  gets the document based on the location in a specific Bucket.
        ///  And extract the text string from it.
        /// </summary>
        public static async Task<string> ExtractTextString(MemoryStream document)
        {
            return await ExtractTextBlocks(document).Select(x => x.Text).ToString("\n");

        }


        /// <summary>
        ///  gets the document based on the location in the s3 bucket configured AWS:Textract:S3Bucket.
        ///  And gives list of detailed blocks of text
        /// </summary>
        public static async Task<List<Block>> ExtractTextBlocks(string documentKey)
        {
            if (BucketName == null)
            {
                throw new KeyNotFoundException("AWS:Textract:S3Bucket missing from your configuration");
            }
            return await ExtractTextBlocks(documentKey, BucketName);

        }

        /// <summary>
        ///  gets the document based on the location in a specific Bucket.
        ///  And gives list of detailed blocks of text
        /// </summary>
        public static async Task<List<Block>> ExtractTextBlocks(string documentKey, string bucketName)
        {
            var detectTextRequest = new DetectDocumentTextRequest()
            {
                Document = new Document()
                {
                    S3Object = new S3Object()
                    {
                        Name = documentKey,
                        Bucket = bucketName
                    }
                }
            };
            try
            {
                var detectTextResponse = await Client.DetectDocumentTextAsync(detectTextRequest);
                return detectTextResponse.Blocks;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        ///  Gets the document using a memorystream.
        ///  And gives list of detailed blocks of text
        /// </summary>
        public static async Task<List<Block>> ExtractTextBlocks(MemoryStream document)
        {
            var detectTextRequest = new DetectDocumentTextRequest(){Document = new Document(){Bytes= document}};

            try
            {
                var detectTextResponse = await Client.DetectDocumentTextAsync(detectTextRequest);
                return detectTextResponse.Blocks;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
    }
}
