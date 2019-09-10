using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    abstract class DotNet : BuildTool
    {
        protected override string Name => "dotnet";

        protected override FileInfo Installer => WindowsCommand.Chocolaty;

        protected override bool AlwaysInstall => true;

        protected override string InstallCommand => "install dotnetcore-sdk --version " + Version;

        public override FileInfo ExpectedPath
            => Environment.SpecialFolder.ProgramFiles.GetFile("dotnet\\dotnet.exe");

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
}