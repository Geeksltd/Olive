﻿using Olive;
using System;
using System.IO;
using System.Threading;

namespace MSharp.Build
{
    class OliveSolution : Builder
    {
        DirectoryInfo Root, Lib;
        readonly bool Publish;
        public bool IsDotNetCore, IsWebForms;

        public OliveSolution(DirectoryInfo root, bool publish)
        {
            Root = root;
            Publish = publish;
            Lib = root.CreateSubdirectory(@"M#\lib");
            IsDotNetCore = IsProjectDotNetCore();
            IsWebForms = root.GetSubDirectory("Website").GetFiles("*.csproj").None();

            if (IsDotNetCore)
                Lib = Lib.GetOrCreateSubDirectory("netcoreapp2.1");
        }

        bool IsProjectDotNetCore()
        {
            return Lib.Parent.GetSubDirectory("Model").GetFile("#Model.csproj").ReadAllText()
                 .Contains("<TargetFramework>netcoreapp");
        }

        protected override void AddTasks()
        {
            Add(() => BuildRuntimeConfigJson());
            Add(() => RestoreNuget());
            Add(() => BuildMSharpModel());
            Add(() => MSharpGenerateModel());
            Add(() => BuildAppDomain());
            Add(() => BuildMSharpUI());
            Add(() => MSharpGenerateUI());
            Add(() => YarnInstall());
            Add(() => InstallBowerComponents());
            Add(() => TypescriptCompile());
            Add(() => SassCompile());
            Add(() => BuildAppWebsite());
        }

        void BuildRuntimeConfigJson()
        {
            var json = @"{  
   ""runtimeOptions"":{  
      ""tfm"":""netcoreapp2.1"",
      ""framework"":{  
         ""name"":""Microsoft.NETCore.App"",
         ""version"":""2.1.0""
      }
   }
}";
            File.WriteAllText(Path.Combine(Lib.FullName, "MSharp.DSL.runtimeconfig.json"), json);
        }

        void RestoreNuget()
        {
            if (!IsDotNetCore)
                WindowsCommand.FindExe("nuget", string.Empty).Execute("restore",
                configuration: x => x.StartInfo.WorkingDirectory = Root.FullName);
        }

        void BuildMSharpModel() => DotnetBuild("M#\\Model");

        void BuildAppDomain() => DotnetBuild("Domain");

        void BuildMSharpUI() => DotnetBuild("M#\\UI");

        void BuildAppWebsite()
        {
            if (IsWebForms)
            {
                RestorePackagesConfig("Website");
                Console.Write("Skipped");
            }
            else DotnetBuild("Website", "publish -o ..\\publish".OnlyWhen(Publish));
        }

        void RestorePackagesConfig(string folder)
        {
            var packages = Folder(folder).AsDirectory().GetFile("packages.config");
            if (packages.Exists())
            {
                WindowsCommand.FindExe("nuget", string.Empty).Execute("restore " + folder + " -packagesdirectory packages",
              configuration: x => x.StartInfo.WorkingDirectory = Root.FullName);
            }
        }

        void DotnetBuild(string folder, string command = null)
        {
            if (IsDotNetCore) DotnetCoreBuild(folder, command);
            else
            {
                RestorePackagesConfig(folder);

                var solution = Root.GetFiles("*.sln")[0].FullName;
                var projName = folder;
                if (folder.StartsWith("M#\\")) projName = "#" + folder.TrimStart("M#\\");

                var dep = " /p:BuildProjectReferences=false".OnlyWhen(folder.StartsWith("M#"));

                WindowsCommand.FindExe("msbuild").Execute($"\"{solution}\" /t:{projName}{dep} -v:m",
                    configuration: x => x.StartInfo.EnvironmentVariables.Add("MSHARP_BUILD", "FULL"));
            }
        }

        void DotnetCoreBuild(string folder, string command = null)
        {
            if (command.IsEmpty()) command = "build -v q";

            var log = WindowsCommand.DotNet.Execute(command,
                configuration: x =>
                {
                    x.StartInfo.WorkingDirectory = Folder(folder);
                    x.StartInfo.EnvironmentVariables.Add("MSHARP_BUILD", "FULL");
                });

            Log(log);
        }

        void MSharpGenerateModel() => RunMSharpBuild("/build /model /no-domain");

        private void MSharpGenerateUI()
        {
            Thread.Sleep(2000);
            RunMSharpBuild("/build /ui");
        }

        void RunMSharpBuild(string command)
        {
            string log;
            if (IsDotNetCore)
            {
                log = WindowsCommand.DotNet.Execute("msharp.dsl.dll " + command,
                   configuration: x => x.StartInfo.WorkingDirectory = Lib.FullName);
            }
            else
            {
                log = Lib.GetFile("MSharp.dsl.exe").Execute(command, configuration: x => x.StartInfo.WorkingDirectory = Lib.FullName);
            }

            Log(log);
        }

        void YarnInstall()
        {
            var log = WindowsCommand.Yarn.Execute("install",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("Website"));
            Log(log);
        }

        void InstallBowerComponents()
        {
            if (!Folder("Website\\bower.json").AsFile().Exists())
            {
                Console.Write("Skipped - bower.json is not found.");
                return;
            }

            var log = WindowsCommand.Bower.Execute("install",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("Website"));
            Log(log);
        }

        void TypescriptCompile()
        {
            var log = WindowsCommand.TypeScript.Execute("",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("Website"));
            Log(log);
        }

        void SassCompile()
        {
            if (!IsDotNetCore) return;

            var log = Folder("Website\\wwwroot\\Styles\\Build\\SassCompiler.exe")
                 .AsFile()
                 .Execute("\"" + Folder("Website\\CompilerConfig.json") + "\"");

            Log(log);
        }

        string Folder(string relative) => Path.Combine(Root.FullName, relative);
    }
}
