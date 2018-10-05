using System.IO;

namespace MSharp.Build.Tools
{
    class Yarn : BuildTool
    {
        protected override string Name => "yarn";
        protected override FileInfo Installer => WindowsCommand.Chocolaty;
        protected override string InstallCommand => "install yarn";
        public override FileInfo ExpectedPath => WindowsCommand.ProgramFiles86("yarn\\bin\\yarn.cmd").AsFile();
    }
}
