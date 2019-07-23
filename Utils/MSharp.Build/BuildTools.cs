using MSharp.Build.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MSharp.Build
{
    class BuildTools : Builder
    {
        protected override void AddTasks()
        {
            Add(() => InstallChocolatey());
            Add(() => InstallDotnetCoreSdk());
            Add(() => InstallReplaceInFiles());
            Add(() => InstallAcceleratePackageRestore());
            Add(() => InstallNodeJs());
            Add(() => InstallYarn());
            Add(() => InstallTypescript());
            Add(() => InstallWebPack());
            Add(() => InstallBower());
        }

        void InstallChocolatey() => WindowsCommand.Chocolaty = Install<Chocolatey>();

        void InstallDotnetCoreSdk()
        {
            WindowsCommand.DotNet = Install<DotNet215>();
            WindowsCommand.DotNet = Install<DotNet22>();
        }

        void InstallReplaceInFiles() => Install<ReplaceInFile>();
        void InstallAcceleratePackageRestore() => Install<AcceleratePackageRestore>();

        void InstallNodeJs() => WindowsCommand.NodeJs = Install<NodeJs>();

        void InstallYarn() => WindowsCommand.Yarn = Install<Yarn>();

        void InstallTypescript() => WindowsCommand.TypeScript = Install<Typescript>();

        void InstallWebPack() => WindowsCommand.WebPack = Install<WebPack>();

        void InstallBower() => WindowsCommand.Bower = Install<Bower>();

        FileInfo Install<T>([CallerMemberName] string step = "") where T : BuildTool, new()
        {
            var builder = new T();
            try { return builder.Install(); }
            finally { Log(string.Join(Environment.NewLine, builder.Logs), step); }
        }
    }
}