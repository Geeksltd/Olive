using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Olive.BlobAws
{
    /// <summary>
    /// Contents of a S3 directory. Files are the keys prefixed by given 'path'.
    /// </summary>
    public class S3DirectoryContents : IDirectoryContents
    {
        readonly IAmazonS3 AmazonS3;
        readonly string BucketName;
        readonly string Subpath;

        IEnumerable<IFileInfo> contents;

        /// <summary>
        /// Initializes a <see cref="S3DirectoryContents"/> instance.
        /// </summary>
        public S3DirectoryContents(IAmazonS3 amazonS3, string bucketName, string subpath)
        {
            AmazonS3 = amazonS3;
            BucketName = bucketName;
            Subpath = subpath.TrimEnd('/') + "/";
        }

        bool IsRoot => Subpath == "/";

        /// <summary>
        /// True if a directory is located at the given path. 
        /// </summary>
        public bool Exists
        {
            get
            {
                try
                {
                    // Root folder always exists
                    if (IsRoot)
                        return true;

                    AmazonS3.GetObjectMetadataAsync(BucketName, Subpath).Wait();
                    return true;
                }
                catch (AggregateException e)
                {
                    e.Handle(ie =>
                    {
                        if (ie is AmazonS3Exception _ie)
                        {
                            if (_ie.StatusCode == HttpStatusCode.NotFound) return true;
                        }

                        return false;
                    });
                    return false;
                }
            }
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            EnumerateContents();
            return contents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            EnumerateContents();
            return contents.GetEnumerator();
        }

        void EnumerateContents()
        {
            var request = new ListObjectsV2Request
            {
                BucketName = BucketName,
                Delimiter = "/",
                Prefix = IsRoot ? "" : Subpath
            };
            var response = AmazonS3.ListObjectsV2Async(request).Result;

            var files = response.S3Objects
                                .Where(x => x.Key != Subpath)
                                .Select(x => new S3FileInfo(AmazonS3, BucketName, x.Key));

            var directories = response.CommonPrefixes
                                      .Select(x => new S3FileInfo(AmazonS3, BucketName, x));

            contents = directories.Concat(files);
        }
    }
}