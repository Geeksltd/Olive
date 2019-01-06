using Amazon.S3;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace Olive.BlobAws
{
    /// <summary>
    /// Looks up files on AWS S3 file system.
    /// </summary>
    public class S3FileProvider : IFileProvider, IDisposable
    {
        static readonly char[] PathSeparators = new[] { '/' };
        static readonly char[] InvalidFileNameChars = "\\{}^%`[]\'\"><~#|".ToCharArray()
            .Concat(Enumerable.Range(128, 255).Select(x => (char)x)).ToArray();

        readonly IAmazonS3 AmazonS3;
        readonly string BucketName;

        /// <summary>
        /// Initializes a new instance of a <see cref="S3FileProvider"/> at the given bucket.
        /// </summary>
        /// <param name="amazonS3"><see cref="IAmazonS3" /> Amazon S3 service object</param>
        /// <param name="bucketName">Name of the bucket that will be used</param>
        public S3FileProvider(IAmazonS3 amazonS3, string bucketName)
        {
            AmazonS3 = amazonS3;
            BucketName = bucketName;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null) throw new ArgumentNullException(nameof(subpath));
            if (HasInvalidFileNameChars(subpath)) return NotFoundDirectoryContents.Singleton;

            // Relative paths starting with leading slashes are okay
            subpath = subpath.TrimStart(PathSeparators);

            return new S3DirectoryContents(AmazonS3, BucketName, subpath);
        }

        /// <summary>
        /// Locates a file at the given path.
        /// </summary>
        /// <param name="subpath">A path under the bucket</param>
        /// <returns>The file information. Caller must check Exists property.</returns>
        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == null)
                throw new ArgumentNullException(nameof(subpath));

            if (HasInvalidFileNameChars(subpath))
                return new NotFoundFileInfo(subpath);

            // Relative paths starting with leading slashes are okay
            subpath = subpath.TrimStart(PathSeparators);

            if (string.IsNullOrEmpty(subpath))
                return new NotFoundFileInfo(subpath);

            return new S3FileInfo(AmazonS3, BucketName, subpath);
        }

        /// <summary>
        /// Watch is not supported.
        /// </summary>
        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        /// <summary>
        /// Disposes the file provider.
        /// </summary>
        public void Dispose() => AmazonS3.Dispose();

        bool HasInvalidFileNameChars(string path) => path.IndexOfAny(InvalidFileNameChars) != -1;
    }
}