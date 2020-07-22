using MSharp.Build.Tools;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MSharp.Build
{
    class BuildTools : Builder
    {
        public BuildTools(bool installAll = true) : base(installAll) { }

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

        void InstallChocolatey() => WindowsCommand.Chocolaty = InstallOrLoad<Chocolatey>();

        void InstallDotnetCoreSdk()
        {
            WindowsCommand.DotNet = InstallOrLoad<DotNet215>();
            WindowsCommand.DotNet = InstallOrLoad<DotNet22>();
        }

        void InstallReplaceInFiles() => InstallOrLoad<ReplaceInFile>();
        void InstallAcceleratePackageRestore() => InstallOrLoad<AcceleratePackageRestore>();

        void InstallNodeJs() => WindowsCommand.NodeJs = InstallOrLoad<NodeJs>();

        void InstallYarn() => WindowsCommand.Yarn = InstallOrLoad<Yarn>();

        void InstallTypescript() => WindowsCommand.TypeScript = InstallOrLoad<Typescript>();

        void InstallWebPack() => WindowsCommand.WebPack = InstallOrLoad<WebPack>();

        void InstallBower() => WindowsCommand.Bower = InstallOrLoad<Bower>();

        FileInfo InstallOrLoad<T>([CallerMemberName] string step = "") where T : BuildTool, new()
        {
            var builder = new T();
            try
            {
                return InstallAll ? builder.Install() : builder.GetActualPath();
            }
            finally { Log(string.Join(Environment.NewLine, builder.Logs), step); }
        }
    }
}