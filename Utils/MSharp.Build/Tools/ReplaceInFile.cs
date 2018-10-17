using System;
using System.IO;

namespace MSharp.Build.Tools
{
    class ReplaceInFile : BuildTool
    {
        protected override string Name => "replace-in-file";
        protected override FileInfo Installer => WindowsCommand.DotNet;
        protected override string InstallCommand => "tool install -g replace-in-file";
        public override FileInfo ExpectedPath => throw new NotImplementedException();
    }
}
