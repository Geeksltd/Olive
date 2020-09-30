using Olive;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace MSharp.Build
{
    class Commands
    {
        static Dictionary<string, FileInfo> Exes = new Dictionary<string, FileInfo>();
        public static FileInfo Chocolaty => FindExe("choco");
        public static FileInfo Yarn => FindExe("yarn");
        public static FileInfo NodeJs => FindExe("npm");
        public static FileInfo TypeScript => FindExe("tsc");
        public static FileInfo WebPack => FindExe("webpack");
        public static FileInfo Bower => FindExe("bower");
        public static FileInfo DotNet => FindExe("dotnet");
        public static FileInfo APT => FindExe("apt");
        public static FileInfo APT_GET => FindExe("apt-get");

        public static FileInfo Where
        {
            get
            {
                if (Runtime.IsWindows())
                    return System32("WHERE.exe").ExistsOrThrow();
                else if (Runtime.IsLinux())
                    return "/usr/bin/whereis".AsFile();

                throw new NotSupportedException(Runtime.OS.ToString());
            }
        }
        public static FileInfo FindExe(string fileInPathEnv)
        {
            if (!Exes.ContainsKey(fileInPathEnv))
            {
                var output = Where.Execute(fileInPathEnv, configuration: x => x.StartInfo.WorkingDirectory = string.Empty);
                Exes.Add(fileInPathEnv, output.Trim().ToLines().Select(x => x.AsFile()).First(x => x.Extension.HasValue()));
            }

            return Exes[fileInPathEnv];
        }

        public static FileInfo Powershell => FindExe("powershell");

        static FileInfo System32(string relative)
        {
            return Environment.SpecialFolder.Windows
                .GetFile("System32\\" + relative)
                .ExistsOrThrow();
        }
    }
}
