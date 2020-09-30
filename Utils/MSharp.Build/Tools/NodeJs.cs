using MSharp.Build.Installers;
using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    class NodeJs : BuildTool
    {
        protected override string Name => "npm";

        protected override Installer LinuxInstaller => new Installers.Linux.APT(Name, "apt install nodejs -yq && apt install npm -yq");

        protected override Installer WindowsInstaller => new Installers.Windows.Chocolaty(Name, "install nodejs.install");

    }
}