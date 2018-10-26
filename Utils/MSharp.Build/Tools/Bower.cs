using System.IO;

namespace MSharp.Build.Tools
{
    class Bower : BuildTool
    {
        protected override string Name => "bower";
        protected override FileInfo Installer => WindowsCommand.Chocolaty;
        protected override string InstallCommand => "install bower";
        public override FileInfo ExpectedPath => WindowsCommand.Roaming("npm\\bower.cmd").AsFile();
    }
}