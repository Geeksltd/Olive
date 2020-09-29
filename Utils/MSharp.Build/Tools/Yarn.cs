using MSharp.Build.Installers;
using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    class Yarn : BuildTool
    {
        protected override string Name => "yarn";

        protected override Installer WindowsInstaller => new Installers.Windows.Chocolaty(Name);

        protected override Installer LinuxInstaller => new Installers.NodeJs(Name);
    }
}
