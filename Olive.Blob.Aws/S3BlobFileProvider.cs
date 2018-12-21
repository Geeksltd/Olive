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
    public class S3BlobFileProvider : IFileProvider, IDisposable
    {
        static readonly char[] PathSeparators = new[] { '/' };
        static readonly char[] InvalidFileNameChars = new[] { '\\', '{', '}', '^', '%', '`', '[', ']', '\'', '"', '>', '<', '~', '#', '|' }
                                                              .Concat(Enumerable.Range(128, 255).Select(x => (char)x))
                                                              .ToArray();

        readonly IAmazonS3 AmazonS3;
        readonly string BucketName;

        /// <summary>
        /// Initializes a new instance of a <see cref="S3BlobFileProvider"/>
        /// </summary>
        public S3BlobFileProvider()
        {
            this.AmazonS3 = AWSInfo.AmazonS3Client;
            this.BucketName = AWSInfo.S3BucketName;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null) throw new ArgumentNullException(nameof(subpath));
            if (HasInvalidFileNameChars(subpath)) return NotFoundDirectoryContents.Singleton;

            // Relative paths starting with leading slashes are okay
            subpath = subpath.TrimStart(PathSeparators);

            return new S3DirectoryContents(subpath);
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

            return new S3BlobFileInfo(subpath);
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