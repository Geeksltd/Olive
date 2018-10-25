using System.IO;

namespace MSharp.Build.Tools
{
    abstract class NetCoreGlobalTool : BuildTool
    {
        public override FileInfo ExpectedPath => WindowsCommand.GlobalDotNetTool($"tools\\{Name}.exe").AsFile();

        protected override FileInfo Installer => WindowsCommand.DotNet;

        protected override string InstallCommand => $"tool install -g {Name}";
    }
}
