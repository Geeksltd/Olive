using Olive;
using System;
using System.IO;
using System.Linq;

namespace MSharp.Build
{
    class WindowsCommand
    {
        public static FileInfo Yarn, Chocolaty, NodeJs, TypeScript, WebPack, Bower, DotNet;

        public static FileInfo Where => System32("WHERE.exe").ExistsOrThrow();

        public static FileInfo FindExe(string fileInPathEnv)
        {
            return Where.Execute(fileInPathEnv).Trim().ToLines()
                        .Select(x => x.AsFile())
                        .First(x => x.Extension.HasValue());
        }

        public static FileInfo Powershell => System32("windowspowershell\\v1.0\\powershell.exe");

        static FileInfo System32(string relative)
        {
            return Environment.SpecialFolder.Windows
                .GetFile("System32\\" + relative)
                .ExistsOrThrow();
        }
    }
}
