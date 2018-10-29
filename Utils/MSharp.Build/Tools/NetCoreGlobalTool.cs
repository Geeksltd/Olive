using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    abstract class NetCoreGlobalTool : BuildTool
    {
        public override FileInfo ExpectedPath =>
                Environment.SpecialFolder.UserProfile.GetFile($".dotnet\\tools\\{Name}.exe");

        protected override FileInfo Installer => WindowsCommand.DotNet;

        protected override string InstallCommand => $"tool install -g {Name}";
    }
}
