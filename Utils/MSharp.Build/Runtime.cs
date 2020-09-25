using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MSharp.Build
{
    static class Runtime
    {
        internal static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        internal static string OS => RuntimeInformation.OSDescription;
    }
}
