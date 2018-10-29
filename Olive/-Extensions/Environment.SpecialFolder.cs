namespace Olive
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    partial class OliveExtensions
    {
        public static DirectoryInfo Directory(this Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder).AsDirectory();
        }

        public static DirectoryInfo Directory(this Environment.SpecialFolder folder,
            string relativeSubDirectory)
        {
            return Path.Combine(
                folder.Directory().FullName,
                relativeSubDirectory.TrimStart("\\"))
                .AsDirectory();
        }

        public static FileInfo GetFile(this Environment.SpecialFolder folder, string relativePath)
        {
            return Path.Combine(folder.Directory().FullName, relativePath.TrimStart("\\")).AsFile();
        }
    }
}