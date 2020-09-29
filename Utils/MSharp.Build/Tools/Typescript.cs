using MSharp.Build.Installers;
using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    class Typescript : BuildTool
    {
        protected override string Name => "tsc";
        protected override Installer LinuxInstaller => WindowsInstaller;
        protected override Installer WindowsInstaller => new Installers.NodeJs(Name);
    }
}
