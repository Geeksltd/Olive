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
            if (Runtime.IsWindows())
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

        void InstallChocolatey() => Install<Chocolatey>();

        void InstallDotnetCoreSdk()
        {
            Install<DotNet31>();
        }

        void InstallReplaceInFiles() => Install<ReplaceInFile>();
        void InstallAcceleratePackageRestore() => Install<AcceleratePackageRestore>();

        void InstallNodeJs() => Install<NodeJs>();

        void InstallYarn() => Install<Yarn>();

        void InstallTypescript() => Install<Typescript>();

        void InstallWebPack() => Install<WebPack>();

        void InstallBower() => Install<Bower>();

        void Install<T>([CallerMemberName] string step = "") where T : BuildTool, new()
        {
            var builder = new T();
            try
            {
                builder.Install();
            }
            finally { Log(string.Join(Environment.NewLine, builder.Logs), step); }
        }
    }
}