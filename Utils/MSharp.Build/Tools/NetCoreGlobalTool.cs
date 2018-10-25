using System.IO;

namespace MSharp.Build.Tools
{
    abstract class NetCoreGlobalTool : BuildTool
    {
        public override FileInfo ExpectedPath => WindowsCommand.LocalNuget($"tools\\{Name}.exe").AsFile();

        protected override FileInfo Installer => WindowsCommand.DotNet;

        protected override string InstallCommand => $"tool install -g {Name}";
    }
}
