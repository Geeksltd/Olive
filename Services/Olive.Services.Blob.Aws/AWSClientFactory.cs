namespace Olive.Services.BlobAws
{
    using Amazon.S3;

    /// <summary>
    /// Factory class that creates Amazong S3 Clients.
    /// </summary>
    static class AWSClientFactory
    {
        /// <summary>
        /// Creates an Amazon S3 Client.
        /// </summary>
        internal static IAmazonS3 CreateS3Client() => AWSClientFactory.CreateS3Client();
    }
}