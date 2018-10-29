using Olive;
using System;
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
            => Environment.SpecialFolder.CommonApplicationData.GetFile("chocolatey\\bin\\choco.exe");

        protected override void OnInstalled()
        {
            if (IsInstalled())
            {
                var log = Path.Execute("feature enable -n allowGlobalConfirmation");
                Logs.Add("Enable allow global feature: " + log);
            }
        }
    }
}
