using MSharp.Build.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MSharp.Build
{
    class BuildTools : Builder
    {
        protected override void AddTasks()
        {
            Add(() => InstallChocolatey());
            Add(() => InstallDotnetCoreSdk());
            Add(() => InstallNodeJs());
            Add(() => InstallYarn());
            Add(() => InstallTypescript());
            Add(() => InstallWebPack());
            Add(() => InstallBower());
        }

        bool IsInstalled(string tool, out FileInfo path)
        {
            try
            {
                path = WindowsCommand.Where.Execute(tool).Trim().ToLines()
                    .Select(x => x.AsFile()).First(x => x.Extension.HasValue());
                return true;
            }
            catch
            {
                path = null;
                return false;
            }
        }

        void InstallChocolatey()
        {
            if (IsInstalled("choco", out WindowsCommand.Chocolaty)) return;

            var command = @"-NoProfile -InputFormat None -ExecutionPolicy Bypass -Command ""iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))""";
            Install(WindowsCommand.Powershell, command, "choco");

            WindowsCommand.Chocolaty = Path.Combine(WindowsCommand.ProgramsData, "chocolatey\\bin\\choco.exe").AsFile();

            AddToPath(WindowsCommand.Chocolaty);

            var log = WindowsCommand.Chocolaty.Execute("feature enable -n allowGlobalConfirmation");
            Log("Enable allow global feature: " + log);
        }

        void InstallDotnetCoreSdk()
        {
            var log = WindowsCommand.Chocolaty.Execute("install dotnetcore-sdk");
            Log(log);

            IsInstalled("dotnet", out WindowsCommand.DotNet);
        }

        void InstallNodeJs()
        {
            WindowsCommand.NodeJs = Install(WindowsCommand.Chocolaty, "install nodejs.install", "npm");
        }

        void InstallYarn()
        {
            WindowsCommand.Yarn = Install(WindowsCommand.Chocolaty, "install yarn", "yarn");
        }

        void InstallTypescript()
        {
            WindowsCommand.TypeScript = Install(WindowsCommand.NodeJs, "install -g typescript", "tsc");
        }

        void InstallWebPack() => Install<WebPack>(out WindowsCommand.WebPack);

        void Install<T>(out FileInfo tool, [CallerMemberName] string step = "") where T : BuildTool, new()
        {
            var builder = new T();
            try { tool = builder.Install(); }
            finally { Log(string.Join(Environment.NewLine, builder.Logs), step); }
        }

        void InstallBower()
        {
            WindowsCommand.Bower = Install(WindowsCommand.Chocolaty, "install bower", "bower");
        }

        FileInfo Install(FileInfo installer, string installCommand, string tool)
        {
            if (IsInstalled(tool, out var path)) return path;

            var log = installer.Execute(installCommand);
            Log(log);

            if (IsInstalled(tool, out path))
            {
                AddToPath(path);
                return path;
            }
            throw new Exception($"Failed to install {tool}. Install it manually.");
        }

        void AddToPath(FileInfo tool)
        {
            var parts =
                Environment.GetEnvironmentVariable("PATH").TrimOrEmpty().Split(';')
            .Concat(new[] { tool.Directory.FullName })
            .Distinct();

            Environment.SetEnvironmentVariable("PATH", string.Join(";", parts));
        }
    }
}