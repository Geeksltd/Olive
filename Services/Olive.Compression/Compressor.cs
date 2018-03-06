using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Olive.Compression
{
    public static class Compressor
    {
        /// <summary>
        /// A fluent API for compressing archives
        /// </summary>
        /// <param name="format">Format of compressed file</param>
        /// <param name="folderPath">The folder path that you want to compress</param>
        /// <param name="destinationFilePath">Desired folder that your compressed file is going to be saved in</param>
        public static void Compress(CompressionFormats format, DirectoryInfo folderPath, FileInfo destinationFilePath)
        {

            if (format == CompressionFormats.zip)
            {
                using (var archive = ZipArchive.Create())
                {
                    archive.AddAllFromDirectory(folderPath.FullName);
                    archive.SaveTo(destinationFilePath.FullName, CompressionType.Deflate);
                }
            }

            else if (format == CompressionFormats.tar)
            {
                using (var archive = TarArchive.Create())
                {
                    archive.AddAllFromDirectory(folderPath.FullName);
                    archive.SaveTo(destinationFilePath.FullName, CompressionType.None);
                }
            }

            else if (format == CompressionFormats.gzip)
            {
                using (var archive = GZipArchive.Create())
                {
                    archive.AddAllFromDirectory(folderPath.FullName);
                    archive.SaveTo(destinationFilePath.FullName, CompressionType.Deflate);
                }
            }

            else
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// This method used for decompressing archives
        /// </summary>
        /// <param name="filePath">The compressed file desired to decompress</param>
        /// <param name="destinationPath">Desired decompression desination path</param>
        /// <param name="extractFullPath"></param>
        /// <param name="overrite"></param>
        public static void Decompress(FileInfo filePath, DirectoryInfo destinationPath, bool extractFullPath = true, bool overrite = true)
        {
            using (Stream stream = File.OpenRead(filePath.FullName))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        Console.WriteLine(reader.Entry.Key);
                        reader.WriteEntryToDirectory(destinationPath.FullName, new ExtractionOptions()
                        {
                            ExtractFullPath = extractFullPath,
                            Overwrite = overrite
                        });
                    }
                }
            }
        }

    }
    public enum CompressionFormats
    {
        zip,
        gzip,
        tar
    }
}
