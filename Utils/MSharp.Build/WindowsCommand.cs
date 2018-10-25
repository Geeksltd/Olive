using System;
using System.IO;

namespace MSharp.Build
{
    class WindowsCommand
    {
        public static FileInfo Yarn, Chocolaty, NodeJs, TypeScript, WebPack, Bower, DotNet;

        public static FileInfo Where => System32("WHERE.exe");

        public static FileInfo Powershell => System32("windowspowershell\\v1.0\\powershell.exe");

        static FileInfo System32(string relative)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Windows) +
                "\\System32\\" + relative;

            if (!File.Exists(path)) throw new Exception("File not found: " + path);

            return new FileInfo(path);
        }

        public static string ProgramsData(string relative)
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), relative);

        public static string LocalAppData(string relative)
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), relative);

        public static string Roaming(string relative)
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), relative);

        public static string ProgramFiles(string relative)
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), relative);

        public static string ProgramFiles86(string relative)
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), relative);

        public static string GlobalDotNetTool(string relative)
        {
            var result = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            result = new DirectoryInfo(result).Parent.FullName;
            return Path.Combine(result, ".dotnet", relative);
        }
    }
}
