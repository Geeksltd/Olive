using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    class DotNet : BuildTool
    {
        protected override string Name => "dotnet";

        protected override FileInfo Installer => WindowsCommand.Chocolaty;

        protected override bool AlwaysInstall => true;

        protected override string InstallCommand => "install dotnetcore-sdk --version 2.1.403";

        public override FileInfo ExpectedPath
            => Environment.SpecialFolder.ProgramFiles.GetFile("dotnet\\dotnet.exe");
    }
}
