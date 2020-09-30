using MSharp.Build.Installers;
using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    abstract class DotNet : BuildTool
    {
        protected override string Name => "dotnet";

        protected override Installer LinuxInstaller => new Installers.Linux.APTGet(Name, "sudo apt-get update; sudo apt-get install -y apt-transport-https && sudo apt-get update && sudo apt-get install -y dotnet-sdk-" + Version);

        protected override Installer WindowsInstaller => new Installers.Windows.Chocolaty(Name, "install dotnetcore-sdk --version " + Version);

        protected override bool AlwaysInstall => true;

        protected abstract string Version { get; }
    }

    class DotNet215 : DotNet
    {
        protected override string Version => "2.1.5";
    }

    class DotNet22 : DotNet
    {
        protected override string Version => "2.2";
    }

    class DotNet31 : DotNet
    {
        protected override string Version => "3.1";
    }
}