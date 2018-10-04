using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSharp.Build
{
    class WindowsCommand
    {
        public static FileInfo Yarn, Chocolaty, NodeJs, TypeScript, WebPack, Bower, DotNet;

        public static FileInfo Where => Get("System32\\WHERE.exe");

        public static FileInfo Powershell => Get("System32\\windowspowershell\\v1.0\\powershell.exe");

        static FileInfo Get(string relative)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Windows) +
                "\\" + relative;

            if (!File.Exists(path))
                throw new Exception("File not found: " + path);

            return new FileInfo(path);
        }

        public static string ProgramsData
            => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    }
}
