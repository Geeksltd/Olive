using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MSharp.Build.Tools
{
    class WebPack : BuildTool
    {
        protected override string Name => "webpack";
        protected override FileInfo Installer => WindowsCommand.Yarn;
        protected override string InstallCommand => "global add webpack";
        public override FileInfo ExpectedPath => WindowsCommand.LocalAppData("Yarn/bin/webpack.cmd").AsFile();
    }
}
