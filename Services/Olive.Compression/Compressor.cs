using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;

namespace Olive
{
    public enum CompressionFormat { Zip, Gzip, Tar }

    public static class CompressionIOExtensions
    {
        /// <summary>
        /// Creates a compressed file from this directory.
        /// </summary>        
        /// <param name="destination">Path of the target zip file to generate.</param>
        public static void Compress(this DirectoryInfo source, CompressionFormat format, FileInfo destination, bool overwrite = false)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            if (destination.Exists() && !overwrite)
                throw new Exception("Destination file already exists: " + destination.FullName);

            IWritableArchive archive;
            var type = CompressionType.Deflate;

            switch (format)
            {
                case CompressionFormat.Zip:
                    archive = ZipArchive.Create();
                    break;
                case CompressionFormat.Gzip:
                    archive = GZipArchive.Create();
                    break;
                case CompressionFormat.Tar:
                    archive = TarArchive.Create();
                    type = CompressionType.None;
                    break;
                default: throw new NotSupportedException();
            }

            using (archive)
            {
                archive.AddAllFromDirectory(source.FullName);
                archive.SaveTo(destination.FullName, type);
            }
        }

        /// <summary>
        /// Decompresses this file into a specified directory.
        /// </summary>
        /// <param name="destination">Desired decompression desination path.</param>
        public static void Decompress(this FileInfo compressedFile, DirectoryInfo destination,
            bool extractFullPath = true, bool overwrite = false)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            if (!compressedFile.Exists())
                throw new IOException("File does not exist: " + compressedFile.FullName);

            if (destination.Exists() && !overwrite)
                throw new Exception("Destination file already exists: " + destination.FullName);

            var options = new ExtractionOptions()
            {
                ExtractFullPath = extractFullPath,
                Overwrite = overwrite
            };

            using (var stream = File.OpenRead(compressedFile.FullName))
            using (var reader = ReaderFactory.Open(stream))
                while (reader.MoveToNextEntry())
                    if (!reader.Entry.IsDirectory)
                        reader.WriteEntryToDirectory(destination.FullName, options);
        }
    }
}