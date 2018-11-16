using System;
using System.Collections.Generic;
using System.IO;

namespace Olive.ApiProxy
{
    abstract class ProjectCreator
    {
        internal virtual string Name => Folder.Name;

        public DirectoryInfo Folder;

        public DirectoryInfo MockFolder => Folder.GetOrCreateSubDirectory("Mock");

        internal static string Version = LocalTime.Now.ToString("yyMMdd.HH.mmss");

        protected abstract string Framework { get; }
        protected abstract string[] References { get; }

        protected ProjectCreator(string name)
        {
            Folder = Context.TempPath.GetOrCreateSubDirectory(Context.ControllerType.FullName + "." + name);
        }

        protected abstract void AddFiles();

        internal abstract string IconUrl { get; }

        void Create()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-------------------");
            Console.ResetColor();

            Console.WriteLine("Creating a new class library project at " + Folder.FullName + "...");

            Folder.GetFile(Name + ".csproj").WriteAllText($@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
      <TargetFramework>{Framework}</TargetFramework>
      <DocumentationFile>{BinFolder}\{Name}.xml</DocumentationFile>
  </PropertyGroup>
</Project>");

            Environment.CurrentDirectory = Folder.FullName;
        }

        void AddNugetReferences()
        {
            foreach (var item in References)
            {
                Console.Write("Adding nuget reference " + item + "...");
                Context.Run("dotnet add package " + item);
                Console.WriteLine("Done");
            }
        }

        internal void Build()
        {
            Create();
            AddFiles();
            AddNugetReferences();

            Console.Write("Building " + Folder + "...");
            Context.Run("dotnet build");
            Console.WriteLine("Done");
        }

        internal virtual IEnumerable<string> GetNugetDependencies()
        {
            yield break;
        }

        string BinFolder => $@"bin\Debug\{Framework}";

        protected string OutputFile(string extension)
        {
            return Folder.GetSubDirectory(BinFolder)
                .GetFile(Name + extension)
                .FullName.TrimStart(Folder.Parent.FullName).TrimStart("\\");
        }

        public virtual IEnumerable<string> GetTargetFiles()
        {
            yield return $@"<file src=""{OutputFile(".dll")}"" target=""lib\{Framework}\"" />";
            yield return $@"<file src=""{OutputFile(".xml")}"" target=""lib\{Framework}\"" />";
        }
    }
}