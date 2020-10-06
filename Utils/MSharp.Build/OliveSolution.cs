using Olive;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

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
            {
                Lib = new[] { "3.1", "2.2", "2.1" }.Select(x => Lib.GetSubDirectory("netcoreapp" + x))
                    .FirstOrDefault(x => x.Exists())
                    ?? throw new Exception("netcoreapp3.1 folder is not found in " + Lib.FullName);
            }
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
      ""tfm"":""VERSION"",
      ""framework"":{  
         ""name"":""Microsoft.NETCore.App"",
         ""version"":""2.MINOR-VER.0""
      }
   }
}".Replace("VERSION", Lib.Name).Replace("MINOR-VER", Lib.Name.Last().ToString());
            File.WriteAllText(Path.Combine(Lib.FullName, "MSharp.DSL.runtimeconfig.json"), json);
        }

        void RestoreNuget()
        {
            if (!IsDotNetCore)
                Commands.FindExe("nuget").Execute("restore",
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
                CopyDllsToWebsite();
            }
            else DotnetBuild("Website", "publish -o ..\\publish".OnlyWhen(Publish));
        }

        private void CopyDllsToWebsite()
        {
            var bin = "Website/bin".AsDirectory();

            var dllPaths = from file in bin.GetFiles("*.refresh")
                           let source = Root.GetSubDirectory("Website").GetFile(file.ReadAllText())
                           let item = new
                           {
                               Source = source,
                               Destination = bin.GetFile(source.Name)
                           }
                           where item.Destination.Exists == false
                           select item;

            dllPaths.Do(p => File.Copy(p.Source.FullName, p.Destination.FullName, true));
        }

        const string PACKAGES_DIRECTORY = "packages";
        void RestorePackagesConfig(string folder)
        {
            var packages = Folder(folder).AsDirectory().GetFile("packages.config");
            if (packages.Exists())
            {
                Commands.FindExe("nuget").Execute("restore " + folder + " -packagesdirectory " + Root.GetOrCreateSubDirectory(PACKAGES_DIRECTORY).FullName,
              configuration: x => x.StartInfo.WorkingDirectory = Root.FullName);
            }
        }

        FileInfo GetPackages(string folder) => Folder(folder).AsDirectory().GetFile("packages.config");


        string GetProjectSolution() => Root.GetFiles("*.sln")[0].FullName;
        void DotnetBuild(string folder, string command = null)
        {
            if (IsDotNetCore) DotnetCoreBuild(folder, command);
            else
            {
                RestorePackagesConfig(folder);

                var solution = GetProjectSolution();
                var projName = folder;
                var project = folder.AsDirectory().GetFiles("*.csproj")[0].FullName;
                if (folder.StartsWith("M#\\")) projName = "#" + folder.TrimStart("M#\\");

                var dep = " /p:BuildProjectReferences=false".OnlyWhen(folder.StartsWith("M#"));

                Commands.FindExe("msbuild").Execute($"\"{project}\" -v:m",
                    configuration: x => x.StartInfo.EnvironmentVariables.Add("MSHARP_BUILD", "FULL"));
            }
        }

        void DotnetCoreBuild(string folder, string command = null)
        {
            if (command.IsEmpty()) command = "build -v q";

            var log = Commands.DotNet.Execute(command,
                configuration: x =>
                {
                    x.StartInfo.WorkingDirectory = Folder(folder);
                    x.StartInfo.EnvironmentVariables.Add("MSHARP_BUILD", "FULL");
                });

            Log(log);
        }

        void MSharpGenerateModel() => RunMSharpBuild("/build /model /no-domain");

        void MSharpGenerateUI() => RunMSharpBuild("/build /ui");

        void RunMSharpBuild(string command)
        {
            string log;
            if (IsDotNetCore)
            {
                log = Commands.DotNet.Execute($"msharp.dsl.dll " + command,
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
            var log = Commands.Yarn.Execute("install",
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

            var log = Commands.Bower.Execute("install",
                configuration: x => x.StartInfo.WorkingDirectory = Folder("Website"));
            Log(log);
        }

        void TypescriptCompile()
        {
            var log = Commands.TypeScript.Execute("",
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

        string Folder(string relative) => Root.GetSubDirectory(relative).FullName;
    }
}