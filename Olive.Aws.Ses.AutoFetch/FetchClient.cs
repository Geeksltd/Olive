using Olive.Entities.Data;
using Olive;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MimeKit;
using System.IO;
using Olive.Entities;

namespace Olive.Aws.Ses.AutoFetch
{
    class FetchClient : IDisposable
    {
        EmailAccount Account;
        Amazon.S3.AmazonS3Client S3Client;
        IDatabase Database => Olive.Context.Current.Database();
        FetchClient()
        {
            Log.For(this).Info("Creating the aws client ...");
            S3Client = new Amazon.S3.AmazonS3Client();
            Log.For(this).Info("Aws client created");
        }

        internal static async Task Fetch(EmailAccount account)
        {
            using (var client = new FetchClient { Account = account })
                await client.Fetch();
        }

        void LogInfo(string log) => Log.For(this).Info(log);

        private async Task Fetch()
        {
            var isEmpty = false;
            while (!isEmpty)
            {
                LogInfo($"Downloading from " + Account.S3Bucket);
                var request = new Amazon.S3.Model.ListObjectsV2Request { BucketName = Account.S3Bucket };
                var response = await S3Client.ListObjectsV2Async(request);

                LogInfo($"Downloaded {response.S3Objects.Count} items from " + Account.S3Bucket);

                foreach (var item in response.S3Objects)
                {
                    await Fetch(item);
                }

                Log.For(this).Info($"Downloaded {response.S3Objects.Count} items from " + Account.S3Bucket);

                isEmpty = response.NextContinuationToken.IsEmpty();

                if (isEmpty)
                {
                    Log.For("Downloaded all the objects from " + Account.S3Bucket);
                    break;
                }
            }
        }

        async Task Fetch(Amazon.S3.Model.S3Object item)
        {
            LogInfo("Downloading object " + item.Key);
            var message = await GetObject(item);
            LogInfo("Downloaded object " + item.Key);

            using (var scope = Database.CreateTransactionScope())
            {
                await Database.Save(message);

                LogInfo("Deleting object " + item.Key);
                await Delete(item);
                LogInfo("Deleted object " + item.Key);

                scope.Complete();
            }

        }

        async Task<IMailMessage> GetObject(Amazon.S3.Model.S3Object item)
        {
            var request = new Amazon.S3.Model.GetObjectRequest { Key = item.Key, BucketName = item.BucketName };
            var response = await S3Client.GetObjectAsync(request);
            var sesMessage = MimeMessage.Load(response.ResponseStream);
            return Account.CreateMailMessage(sesMessage);
        }

        Task Delete(Amazon.S3.Model.S3Object item)
        {
            var request = new Amazon.S3.Model.DeleteObjectRequest { BucketName = item.BucketName, Key = item.Key };

            return S3Client.DeleteObjectAsync(request);
        }

        public void Dispose()
        {
            S3Client?.Dispose();
        }
    }
}
