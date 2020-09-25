using Olive;
using System;
using System.IO;
using System.Linq;

namespace MSharp.Build
{
    class WindowsCommand
    {
        static FileInfo _Chocolaty;
        public static FileInfo Chocolaty
        {
            get
            {
                if (!Runtime.IsWindows())
                    throw new Exception("Choco is only supported on Windows. It is not available for " + Runtime.OS);
                return _Chocolaty;
            }
            set
            {
                if (!Runtime.IsWindows())
                    throw new Exception("Choco is only supported on Windows. It is not available for " + Runtime.OS);
                _Chocolaty = value;
            }
        }
        public static FileInfo Yarn, NodeJs, TypeScript, WebPack, Bower, DotNet;

        public static FileInfo Where => System32("WHERE.exe").ExistsOrThrow();

        public static FileInfo FindExe(string fileInPathEnv)
        {
            var output = Where.Execute(fileInPathEnv, configuration: x => x.StartInfo.WorkingDirectory = string.Empty);
            return output.Trim().ToLines().Select(x => x.AsFile()).First(x => x.Extension.HasValue());
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
