using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MSharp.Build
{
    static class Runtime
    {
        internal static bool IsWindows() => OS == OSPlatform.Windows;
        internal static bool IsLinux() => OS == OSPlatform.Linux;

        internal static OSPlatform OS
        {
            get
            {
                var result = new[]
                {
                    OSPlatform.Windows,
                    OSPlatform.Linux
                }.FirstOrDefault(os => RuntimeInformation.IsOSPlatform(os));
                if (result == null)
                    throw new NotSupportedException(RuntimeInformation.OSDescription);

                return result;
            }
        }
    }
}
