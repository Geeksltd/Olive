using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    class Typescript : BuildTool
    {
        protected override string Name => "tsc";
        protected override FileInfo Installer => WindowsCommand.NodeJs;
        protected override string InstallCommand => "install -g typescript";

        public override FileInfo ExpectedPath
            => Environment.SpecialFolder.ApplicationData.GetFile("npm\\tsc.cmd");
    }
}
