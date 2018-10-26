using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace MSharp.Build
{
    class OliveSolution : Builder
    {
        const string ModelProjectName = "#Model", UIProjectName = "#UI", DomainProjectName = "Domain", WebsiteProjectName = "Website";

        DirectoryInfo Root, Lib;
        bool Publish, ModelBuilt, UIBuilt;
        SolutionFile SolutionFile;

        public OliveSolution(DirectoryInfo root, bool publish)
        {
            Root = root;
            Publish = publish;
            Lib = root.CreateSubdirectory(@"M#\lib\netcoreapp2.1\");

            SolutionFile = SolutionFile.Parse(Root.GetFiles("*.sln").FirstOrDefault()?.FullName ??
                throw new InvalidOperationException($"There is no solution file in the current working directory. '{Root.FullName}'"));
        }

        protected override void AddTasks()
        {
            Add(() => BuildRuntimeConfigJson());

            foreach (var project in GetSortedProject())
            {
                var name = project.ProjectName;

                if (name == ModelProjectName)
                    AddModelBuilds();

                else if (name == UIProjectName)
                    AddUIBuilds();

                else if (name == DomainProjectName)
                {
                    AddModelBuilds();
                    Add(() => BuildAppDomain());
                }
                else if (name == WebsiteProjectName)
                {
                    AddWebsitePreparations();
                    Add(() => BuildAppWebsite());
                }
                else
                    Add($"Build{name}", () => DotnetBuild(project.RelativePath.Replace($"\\{name}.csproj", "")));
            }
        }

        IEnumerable<ProjectInSolution> GetSortedProject()
        {
            ProjectInSolution[] projects = SolutionFile.ProjectsInOrder
                .Where(p =>
                    p.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat ||
                    p.ProjectType == SolutionProjectType.WebProject)
                .ToArray();

            var projectComparision = new Comparison<ProjectInSolution>((left, right) =>
            {
                var leftDeps = GetDependencies(left).ToArray();
                var rightDeps = GetDependencies(right).ToArray();

                if (leftDeps.Contains(right.ProjectName)) return 1;

                if (rightDeps.Contains(left.ProjectName)) return -1;

                return leftDeps.Count().CompareTo(rightDeps.Count());
            });

            Array.Sort(projects, projectComparision);

            return projects;
        }

        IEnumerable<string> GetDependencies(ProjectInSolution project)
        {
            var xml = new XmlDocument();

            xml.Load(project.AbsolutePath);

            foreach (XmlNode dependency in xml.GetElementsByTagName("ProjectReference"))
            {
                var fileInfo = dependency.Attributes["Include"].Value.AsFile();

                var dependencyName = fileInfo.Name.Replace(fileInfo.Extension, "");

                yield return dependencyName;

                foreach (var innerDependency in GetDependencies(SolutionFile.ProjectsInOrder.FirstOrDefault(p => p.ProjectName == dependencyName)))
                    yield return innerDependency;
            }
        }



        void AddWebsitePreparations()
        {
            AddUIBuilds();
            Add(() => YarnInstall());
            Add(() => InstallBowerComponents());
            Add(() => TypescriptCompile());
            Add(() => SassCompile());
        }

        void AddUIBuilds()
        {
            if (UIBuilt) return;

            UIBuilt = true;

            Add(() => BuildMSharpUI());
            Add(() => MSharpGenerateUI());
        }

        void AddModelBuilds()
        {
            if (ModelBuilt) return;

            ModelBuilt = true;

            Add(() => BuildMSharpModel());
            Add(() => MSharpGenerateModel());
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