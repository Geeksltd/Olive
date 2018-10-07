using System.IO;

namespace MSharp.Build.Tools
{
    class Chocolatey : BuildTool
    {
        protected override string Name => "choco";
        protected override FileInfo Installer => WindowsCommand.Powershell;

        protected override string InstallCommand
            => @"-NoProfile -InputFormat None -ExecutionPolicy Bypass -Command ""iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))""";

        public override FileInfo ExpectedPath
            => WindowsCommand.ProgramsData("chocolatey\\bin\\choco.exe").AsFile();

        protected override void OnInstalled()
        {
            var log = WindowsCommand.Chocolaty.Execute("feature enable -n allowGlobalConfirmation");
            Logs.Add("Enable allow global feature: " + log);
        }
    }
}
