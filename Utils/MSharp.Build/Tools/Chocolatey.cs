using MSharp.Build.Installers;
using MSharp.Build.Installers.Windows;
using Olive;
using System;
using System.IO;

namespace MSharp.Build.Tools
{
    class Chocolatey : BuildTool
    {
        protected override Installer LinuxInstaller => null;
        protected override Installer WindowsInstaller => new Powershell(Name, @"-NoProfile -InputFormat None -ExecutionPolicy Bypass -Command ""iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))""");

        protected override string Name => "choco";

        protected override void OnInstalled()
        {
            if (Installer.IsInstalled())
            {
                var log = Commands.Chocolaty.Execute("feature enable -n allowGlobalConfirmation");
                Logs.Add("Enable allow global feature: " + log);
            }
        }
    }
}
