using System;
using System.Collections.Generic;
using System.IO;
using Olive;

namespace MSharp.Build.Installers
{
    class DotNet : Installer
    {
        internal DotNet(string name, string installCommand = null) : base(name, installCommand ?? "tool install -g " + name)
        {
        }

        protected override FileInfo Executable => Commands.DotNet;
    }
}
