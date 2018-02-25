using System;
using System.IO;
using System.Linq;

namespace Olive.ApiProxy
{
    abstract class ProjectCreator
    {
        string Name;
        public DirectoryInfo Folder;

        static string Version = DateTime.Now.ToString("yyMMdd.HH.mmss");

        protected abstract bool AddXml { get; }
        protected abstract string Framework { get; }
        protected abstract string[] References { get; }
        protected abstract void AddFiles();
        protected abstract string IconUrl { get; }

        protected ProjectCreator(string name)
        {
            Name = name;
            Folder = Context.TempPath.GetOrCreateSubDirectory(Context.TempPath.Name + "." + name);
        }

        void Create()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-------------------");
            Console.ResetColor();

            Console.Write("Creating a new class library project at " + Folder.FullName + "...");
            Context.Run($"dotnet new classlib -o {Folder.FullName} -f {Framework} --force ");
            foreach (var f in Folder.GetFiles("Class1.cs")) f.Delete();
            Console.WriteLine("Done");

            Environment.CurrentDirectory = Folder.FullName;
            foreach (var item in References)
            {
                Console.Write("Adding nuget reference " + item + "...");
                Context.Run("dotnet add package " + item);
                Console.WriteLine("Done");
            }

            if (AddXml)
            {
                var file = Folder.GetFiles("*.csproj").Single();
                var content = file.ReadAllText().ToLines().ToList();
                content.Insert(content.IndexOf(x => x.Trim().StartsWith("<TargetFramework")) + 1,
                    $@"    <DocumentationFile>bin\Debug\{Framework}\{Context.ControllerName}.{Name}.xml</DocumentationFile>"
                    );
                file.WriteAllText(content.ToLinesString());
            }
        }

        internal void Build()
        {
            Create();
            AddFiles();

            Console.Write("Building " + Folder + "...");
            Context.Run("dotnet build");
            Console.WriteLine("Done");

            Console.Write("Creating Nuget package for " + Folder.Name + "...");
            CreateNuspec();
            CreateNugetPackage();
            Console.WriteLine("Done");
        }

        void CreateNugetPackage()
        {
            // RunCommand("dotnet pack --no-dependencies -o \"" + Context.Output + "\"");
            Environment.CurrentDirectory = Folder.FullName;
            Context.Run("nuget.exe pack Package.nuspec");

            var package = Folder.GetFiles("*.nupkg").FirstOrDefault()
                ?? throw new Exception("Nuget package was not succesfully generated.");

            if (Context.Output != null)
                package.CopyTo(Context.Output.GetFile(package.Name));
            else
            {
                Context.Run($"nuget.exe push {package.Name} {Context.NugetApiKey} -s {Context.NugetServer}");
            }
        }

        void CreateNuspec()
        {
            var dll = $@"bin\Debug\{Framework}\{Folder.Name}";

            var xml = $@"<file src=""{dll}.xml"" target=""lib\{Framework}\"" />".OnlyWhen(AddXml);

            var nuspec = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
  <metadata>
    <id>{Folder.Name}</id>
    <version>{Version}</version>
    <title>{Folder.Name}</title>
    <authors>Olive Api Proxy Generator</authors>
    {IconUrl.WithWrappers("<iconUrl>", "</iconUrl>")}
    <description>Provides an easy method to invoke the Api functions of {Context.ControllerName}</description>
  </metadata>
  <files>
    <file src=""{dll}.dll"" target=""lib\{Framework}\"" />
    {xml}
  </files>
</package>";

            Folder.GetFile("Package.nuspec").WriteAllText(nuspec);
        }
    }
}