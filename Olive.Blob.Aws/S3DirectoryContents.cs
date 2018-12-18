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
        readonly IAmazonS3 amazonS3;
        readonly string bucketName;
        readonly string subpath;

        IEnumerable<IFileInfo> contents;

        /// <summary>
        /// Initializes a <see cref="S3DirectoryContents"/> instance.
        /// </summary>
        public S3DirectoryContents(IAmazonS3 amazonS3, string bucketName, string subpath)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = bucketName;
            this.subpath = subpath.TrimEnd('/') + "/";
        }

        bool isRoot => subpath == "/";

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
                    if (isRoot)
                        return true;

                    amazonS3.GetObjectMetadataAsync(bucketName, subpath).Wait();
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

        /// <inheritdoc />
        public IEnumerator<IFileInfo> GetEnumerator()
        {
            enumerateContents();
            return contents.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            enumerateContents();
            return contents.GetEnumerator();
        }

        void enumerateContents()
        {
            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Delimiter = "/",
                Prefix = isRoot ? "" : subpath
            };
            var response = amazonS3.ListObjectsV2Async(request).Result;

            var files = response.S3Objects
                                .Where(x => x.Key != subpath)
                                .Select(x => new S3FileInfo(amazonS3, bucketName, x.Key));

            var directories = response.CommonPrefixes
                                      .Select(x => new S3FileInfo(amazonS3, bucketName, x));

            contents = directories.Concat(files);
        }
    }
}