using System.IO;

namespace MSharp.Build.Tools
{
    class DotNet : BuildTool
    {
        protected override string Name => "dotnet";

        protected override FileInfo Installer => WindowsCommand.Chocolaty;

        protected override bool AlwaysInstall => true;

        protected override string InstallCommand => "install dotnetcore-sdk";

        public override FileInfo ExpectedPath => WindowsCommand.ProgramFiles("dotnet\\dotnet.exe").AsFile();
    }
}
