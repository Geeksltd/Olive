using Olive;
using System;
using System.IO;

namespace MSharp.Build
{
    class WindowsCommand
    {
        public static FileInfo Yarn, Chocolaty, NodeJs, TypeScript, WebPack, Bower, DotNet;

        public static FileInfo Where => System32("System32\\WHERE.exe").ExistsOrThrow();

        public static FileInfo Powershell => System32("windowspowershell\\v1.0\\powershell.exe");

        static FileInfo System32(string relative)
        {
            return Environment.SpecialFolder.Windows
                .GetFile("System32\\" + relative)
                .ExistsOrThrow();
        }
    }
}
