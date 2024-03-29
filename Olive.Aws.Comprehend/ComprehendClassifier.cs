﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using static System.Net.Mime.MediaTypeNames;
using Amazon.S3.Model;
using System.Linq;
using System.Xml.Linq;

namespace Olive.Aws.Comprehend
{
    public static class ComprehendClassifier
    {
        private static IAmazonComprehend _client;
        private static IAmazonS3 _s3client;
        static string BucketName => Config.Get<string>("AWS:Comprehend:S3Bucket").Or(Config.Get<string>("Blob:S3:Bucket"));
        static string ClassificationJobsBucket => Config.Get<string>("AWS:Comprehend:S3ClassificationJobsBucket").Or(Config.Get<string>("Blob:S3:Bucket"));
        private static string _region = Config.Get<string>("AWS:Comprehend:Region").Or(Config.Get<string>("Aws:Region"));
        static string IamRole => Config.Get<string>("AWS:Comprehend:IAMRoleArn");



        public static IAmazonS3 S3Client =>
            _s3client ?? new AmazonS3Client(_region == null?RegionEndpoint.EUWest1: RegionEndpoint.GetBySystemName(_region));

        public static IAmazonComprehend Client => _client ?? 
            Context.Current.GetOptionalService<IAmazonComprehend>() ?? new AmazonComprehendClient(_region == null ? RegionEndpoint.EUWest1 : RegionEndpoint.GetBySystemName(_region));

        /// <summary>
        ///  Creates and uses a new Aws Client in the specified region.
        /// </summary>
        public static void Region(Amazon.RegionEndpoint region) {
                
            SetClient(new AmazonComprehendClient(region));
            _s3client = new AmazonS3Client(region);
         }
        private static void SetClient(AmazonComprehendClient client)
        {
            _client = client;
        }

        /// <summary>
        ///  Creates and starts the training of Comprehend classification module based on AWS:Comprehend:S3Bucket bucket.
        ///  Returns the Amazon arn of the module needed to check the status and use the module.
        ///  If Bucket is not specified will use Blob:S3:Bucket defualt bucket of the application.
        /// </summary>
        public static async Task<string> CreateAsync(string name, string Version, LanguageCode languageCode = null, DocumentClassifierMode mode = null)
        {
            if (BucketName == null)
            {
                throw new KeyNotFoundException("AWS:Comprehend:S3Bucket missing from your configuration");
            }
            return await CreateAsync(name, Version, BucketName, languageCode, mode);

        }
        /// <summary>
        ///  Creates and starts the training of Comprehend classification module based on the bucket specified.
        ///  Returns the Amazon arn of the module needed to check the status and use the module.
        /// </summary>
        public static  async Task<string> CreateAsync(string name, string Version, string bucketName, LanguageCode languageCode= null, DocumentClassifierMode  mode = null)
        {
            if (languageCode == null) mode = DocumentClassifierMode.MULTI_CLASS;
            if (languageCode == null) languageCode = LanguageCode.En;
            if (IamRole == null)
            {
                throw new KeyNotFoundException("AWS:Comprehend:IAMRoleArn missing from your configuration");
            }

            var classifierRequest = new CreateDocumentClassifierRequest()
            {
                VersionName = Version,
                DocumentClassifierName = name + "-" + Version,
                LanguageCode = languageCode,
                InputDataConfig = new DocumentClassifierInputDataConfig() { S3Uri = $"s3://{bucketName}/{name}/{Version}"},
                Mode = mode,
                DataAccessRoleArn = IamRole
            };
            try
            {
                var CreateClassifications = await Client.CreateDocumentClassifierAsync(classifierRequest);
                return CreateClassifications.DocumentClassifierArn;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

        }
        /// <summary>
        ///  Describes the full details of the Classifier.
        /// </summary>
        public static async Task<string> GetClasifierStatus(string classifier_arn)
        {
            var calassifier = await DescribeClasifier(classifier_arn);
            return calassifier.Status;
        }
        /// <summary>
        ///  Describes the full details of the Classifier.
        /// </summary>
        public static async Task<DocumentClassifierProperties> DescribeClasifier(string classifier_arn)
        {
            var classifierRequest = new DescribeDocumentClassifierRequest()
            {
                DocumentClassifierArn = classifier_arn
            };
            try
            {
                var CreateClassifications = await Client.DescribeDocumentClassifierAsync(classifierRequest);
                return CreateClassifications.DocumentClassifierProperties;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

        }
        /// <summary>
        /// Deletes a document classifier.
        /// </summary>
        public static async Task DeleteClassifier(string classifier_arn)
        {
            var classifierRequest = new DeleteDocumentClassifierRequest()
            {
                DocumentClassifierArn = classifier_arn
            };
            try
            {
                var CreateClassifications = await Client.DeleteDocumentClassifierAsync(classifierRequest);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
        /// <summary>
        ///  Describes the full details of the Classifier.
        ///  classifying by endpoint is really expensive use the StartClasifyingJob instead
        public static async Task<DocumentClass> ClassifyByEndpoint(string endpoint_arn, string documentText)
        {
            var classifyRequest = new ClassifyDocumentRequest
            {
                Text = documentText,
                EndpointArn = endpoint_arn,
            };
            try
            {
                var CreateClassifications = await Client.ClassifyDocumentAsync(classifyRequest);
                return CreateClassifications.Classes.FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
        /// <summary>
        ///  Copies the file from AWS:Rekognition:S3SourceBucket to AWS:Comprehend:S3Bucket bucket that is the main comprehend bucket.
        ///  Returns the Checksum SHA256 of the saved file for confirmation if needed.
        ///  If Bucket is not specified will use Blob:S3:Bucket defualt bucket of the application.
        /// </summary>
        public static async Task<string> CopyFile(string sourceKey, string Destinationkey)
        {
            if (BucketName == null)
            {
                throw new KeyNotFoundException("AWS:Comprehend:S3Bucket missing from your configuration for the destination.");
            }
            return await CopyFile(sourceKey, Destinationkey, BucketName, BucketName);
        }
        /// <summary>
        ///  Copies the file from specified source bucket and destination bucket to copy a file.
        ///  Returns the Checksum SHA256 of the saved file for confirmation if needed.
        /// </summary>
        public static async Task<string> CopyFile(string sourceKey, string Destinationkey, string sourceBucket, string destinationBucket)
        {

            var request = new CopyObjectRequest()
            {
                SourceBucket = sourceBucket,
                SourceKey = sourceKey,
                DestinationBucket = destinationBucket,
                DestinationKey = Destinationkey
            };
            try
            {
                var CopyResponse = await S3Client.CopyObjectAsync(request);
                return CopyResponse.ChecksumSHA256;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
        /// <summary>
        ///  Starts the Clasifying job based on the location in the s3 bucket configured AWS:Comprehend:S3ClassificationJobsBucket.
        ///  Gives back the job id that you can use with GetClasifyingJobResults() to get the results of a job.
        /// </summary>
        public static async Task<string> StartClasifyingJob(string classifier_arn, string documetnKey, string outputfolder, string inputfolder)
        {
            if (ClassificationJobsBucket == null)
            {
                throw new KeyNotFoundException("AWS:Comprehend:S3ClassificationJobsBucket missing from your configuration");
            }
            return await StartClasifyingJob(classifier_arn, documetnKey, outputfolder, inputfolder, ClassificationJobsBucket);

        }

        /// <summary>
        ///  Starts the Clasifying job. 
        ///  Gives back the job id that you can use with GetClasifyingJobResults() to get the results of a job.
        /// </summary>
        public static async Task<string> StartClasifyingJob(string classifier_arn, string documetnKey, string outputfolder, string inputfolder, string bucketName)
        {
            if (IamRole == null)
            {
                throw new KeyNotFoundException("AWS:Comprehend:IAMRoleArn missing from your configuration");
            }

            var startJobRequest = new StartDocumentClassificationJobRequest()
            {
                DocumentClassifierArn = classifier_arn,
                InputDataConfig = new InputDataConfig
                {
                    S3Uri = $"s3://{bucketName}/{inputfolder}/{documetnKey}",
                },
                OutputDataConfig = new OutputDataConfig
                {
                    S3Uri = $"s3://{bucketName}/{outputfolder}/{documetnKey}",
                },
                DataAccessRoleArn = IamRole,
            };
            try
            {
                var startJobResponse = await Client.StartDocumentClassificationJobAsync(startJobRequest);
                return startJobResponse.JobId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
        /// <summary>
        ///  Returns an Object containing the status of the Clasifying Job and the Uri of the output File.
        /// </summary>
        public static async Task<DocumentClassificationJobProperties> GetClasifyingJobResults(string jobId, string nextToken = null)
        {
            var clasiifyingJobRequest = new DescribeDocumentClassificationJobRequest()
            {
                JobId = jobId,
            };
            try
            {
                var clasiifyingJobResponse = await Client.DescribeDocumentClassificationJobAsync(clasiifyingJobRequest);
                return clasiifyingJobResponse.DocumentClassificationJobProperties;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }
    }
}
