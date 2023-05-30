using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Textract;
using Amazon.Textract.Model;

namespace Olive.Aws.Textract
{
    public static class AmazonTextract
    {
        private static IAmazonTextract _client;
        static string BucketName => Config.Get<string>("AWS:Rekognition:S3Bucket").Or(Config.Get<string>("Blob:S3:Bucket"));
        private static string _region = Config.Get<string>("AWS:Rekognition:Region").Or(Config.Get<string>("Aws:Region"));
        static string OutputBucketName => Config.Get<string>("AWS:Rekognition:S3OutputBucket").Or(Config.Get<string>("AWS:Rekognition:S3Bucket").Or(Config.Get<string>("Blob:S3:Bucket")));


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
                    },
                    
                },
                
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

        /// <summary>
        ///  Starts the text extraction job based on the location in the s3 bucket configured AWS:Textract:S3Bucket.
        ///  Gives back the job id that you can use with GetJobResultBlocks() or GetJobResultText() to get the results of a job.
        /// </summary>
        public static async Task<string> StartExtraction(string documentKey, string prefix)
        {
            if (BucketName == null)
            {
                throw new KeyNotFoundException("AWS:Textract:S3Bucket missing from your configuration");
            }
            return await StartExtraction(documentKey, prefix, BucketName);

        }

        /// <summary>
        ///  Starts the text extraction job. 
        ///  Gives back the job id that you can use with GetJobResults() to get the results of a job.
        /// </summary>
        public static async Task<string> StartExtraction(string documentKey, string prefix, string bucketName)
        {
            var outputBucket = OutputBucketName.HasValue() ? OutputBucketName : bucketName;
            var detectTextRequest = new StartDocumentTextDetectionRequest()
            {
                DocumentLocation = new DocumentLocation()
                {

                    S3Object = new S3Object()
                    {
                        Name = documentKey,
                        Bucket = bucketName
                    },
                    
                },
                OutputConfig = new OutputConfig()
                {
                    S3Bucket = outputBucket,
                    S3Prefix= prefix,
                    
                },
                
            };
            try
            {
                var detectTextResponse = await Client.StartDocumentTextDetectionAsync(detectTextRequest);
                return detectTextResponse.JobId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
        /// <summary>
        ///  Returns an Object containing the job status blocks and next token of the job.
        /// </summary>
        public static async Task<TextDetectionBlockResults> GetJobResultBlocks(string jobId, string nextToken = null)
        {
            var detectTextRequest = new GetDocumentTextDetectionRequest()
            {
                JobId= jobId,
                NextToken = nextToken
            };
            try
            {
                var detectTextResponse = await Client.GetDocumentTextDetectionAsync(detectTextRequest);
                return new TextDetectionBlockResults(detectTextResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
        /// <summary>
        ///  Returns an Object containing the job status, text and next token of the job.
        /// </summary>
        public static async Task<TextDetectionTextResults> GetJobResultText(string jobId, string nextToken = null)
        {
            var detectTextRequest = new GetDocumentTextDetectionRequest()
            {
                JobId = jobId,
                NextToken = nextToken
            };
            try
            {
                var detectTextResponse = await Client.GetDocumentTextDetectionAsync(detectTextRequest);
                return new TextDetectionTextResults(detectTextResponse); ;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
        /// <summary>
        ///  Starts the text extraction job based on the location in the s3 bucket configured AWS:Textract:S3Bucket.
        ///  Gives back the job id that you can use with GetJobResultBlocks() or GetJobResultText() to get the results of a job.
        /// </summary>
        public static Task<StartDocumentAnalysisResponse> StartAnalyzeDocument(string documentKey, string prefix)
        {
            if (BucketName == null)
            {
                throw new KeyNotFoundException("AWS:Textract:S3Bucket missing from your configuration");
            }


            return StartAnalyzeDocument(documentKey, BucketName, prefix, new List<string> { FeatureType.FORMS });
        }

        /// <summary>
        ///  Starts the text extraction job. 
        ///  Gives back the job id that you can use with GetJobResults() to get the results of a job.
        /// </summary>
        public static Task<StartDocumentAnalysisResponse> StartAnalyzeDocument(string documentKey, string bucketName, string prefix, List<string> featureTypes)
        {
            var outputBucket = OutputBucketName.HasValue() ? OutputBucketName : bucketName;
            var detectTextRequest = new StartDocumentAnalysisRequest()
            {
                DocumentLocation = new DocumentLocation()
                {

                    S3Object = new S3Object()
                    {
                        Name = documentKey,
                        Bucket = bucketName
                    },
                },
                OutputConfig = new OutputConfig()
                {
                    S3Bucket = outputBucket,
                    S3Prefix = prefix,

                },
                FeatureTypes = featureTypes,
            };
            try
            {
                return Client.StartDocumentAnalysisAsync(detectTextRequest);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
    }
}
