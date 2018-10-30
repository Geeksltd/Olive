using Olive;
using System;
using System.IO;

namespace MSharp.Build
{
    class OliveSolution : Builder
    {
        DirectoryInfo Root, Lib;
        bool Publish;

        public OliveSolution(DirectoryInfo root, bool publish)
        {
            Root = root;
            Publish = publish;
            Lib = root.CreateSubdirectory(@"M#\lib\netcoreapp2.1\");
        }

        protected override void AddTasks()
        {
            Add(() => BuildRuntimeConfigJson());
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

        void BuildMSharpModel() => DotnetBuild("M#\\Model");

        void BuildAppDomain() => DotnetBuild(Folder("Domain"));

        void BuildMSharpUI() => DotnetBuild("M#\\UI");

        void BuildAppWebsite()
            => DotnetBuild("Website", "publish -o ..\\publish".OnlyWhen(Publish));

        void DotnetBuild(string folder, string command = null)
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

        void MSharpGenerateModel()
        {
            var log = WindowsCommand.DotNet.Execute($"msharp.dsl.dll /build /model /no-domain",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("M#\\lib\\netcoreapp2.1"));
            Log(log);
        }

        void MSharpGenerateUI()
        {
            var log = WindowsCommand.DotNet.Execute($"msharp.dsl.dll /build /ui",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("M#\\lib\\netcoreapp2.1"));
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
            if (!Folder("Website\\bower.json").AsFile().Exists)
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
            var log = Folder("Website\\wwwroot\\Styles\\Build\\SassCompiler.exe")
                 .AsFile()
                 .Execute("\"" + Folder("Website\\CompilerConfig.json") + "\"");

            Log(log);
        }

        string Folder(string relative) => Path.Combine(Root.FullName, relative);
    }
}