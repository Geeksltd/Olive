using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    public static class ProxyDLLGenerator
    {
        static void PrepareOutputDirectory()
        {
            if (!Context.TempPath.Exists)
                throw new Exception("Output directory not found: " + Context.TempPath.FullName);

            Context.TempPath = new DirectoryInfo(Path.Combine(Context.TempPath.FullName, Context.ControllerName + ".Proxy"));

            try
            {
                if (Context.TempPath.Exists)
                    Context.TempPath.Delete(recursive: true, harshly: true).Wait();
                Context.TempPath.Create();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete the previous output directory " + Context.TempPath.FullName +
                    Environment.NewLine + ex.Message);
            }
        }

        public static void Generate()
        {
            LoadAssembly();

            Console.WriteLine("Generating the proxy class ...");
            var proxyClassCode = ProxyClassProgrammer.Generate();

            PrepareOutputDirectory();

            CreateNewProject();

            Console.Write("Adding the proxy class...");
            File.WriteAllText(Context.TempPath + @"\" + Context.ControllerName + ".cs", proxyClassCode);
            Console.WriteLine("Done");

            DtoProgrammer.CreateDtoTypes();

            Console.Write("Building the generated project...");
            RunCommand("dotnet build");
            Console.WriteLine("Done");

            Console.Write("Creating Nuget package...");
            CreateNuspec();
            //RunCommand("dotnet pack --no-dependencies -o \"" + Context.Output + "\"");
            RunCommand("nuget.exe pack Package.nuspec");

            var package = Context.TempPath.GetFiles("*.nupkg").FirstOrDefault();
            if (package == null) throw new Exception("Nuget package was not succesfully generated.");
            package.CopyTo(Context.Output.GetFile(package.Name));

            Console.WriteLine("Done");
        }

        static void CreateNuspec()
        {
            var version = DateTime.Now.ToString("yyMMdd.HH.mmss");

            var nuspec = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
  <metadata>
    <id>{Context.ControllerName}</id>
    <version>{version}</version>
    <title>{Context.ControllerName}</title>
    <authors>Olive Api Proxy Generator</authors>    
    <description>Provides an easy method to invoke the Api functions of {Context.ControllerName}</description>
  </metadata>
  <files>
    <file src=""bin\Debug\netstandard2.0\{Context.ControllerName}.Proxy.dll"" target=""lib\netstandard2.0\"" />
    <file src=""bin\Debug\netstandard2.0\{Context.ControllerName}.Proxy.pdb"" target=""lib\netstandard2.0\"" />
    <file src=""bin\Debug\netstandard2.0\{Context.ControllerName}.Proxy.xml"" target=""lib\netstandard2.0\"" />
  </files>
</package>";

            Context.TempPath.GetFile("Package.nuspec").WriteAllText(nuspec);
        }

        static void LoadAssembly()
        {
            if (!Context.AssemblyFile.Exists)
                throw new Exception("File not found: " + Context.AssemblyFile.FullName);

            Context.Assembly = Assembly.LoadFrom(Context.AssemblyFile.FullName);
            Context.ControllerType = Context.Assembly.GetType(Context.ControllerName)
                ?? throw new Exception(Context.ControllerName + " was not found.");

            Context.ActionMethods = Context.ControllerType
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Select(x => new MethodGenerator(x))
                .ToArray();

            if (Context.ActionMethods.Length == 0) throw new Exception("This controller has no action method.");
        }

        static void CreateNewProject()
        {
            Console.Write("Creating a new class library project at " + Context.TempPath.FullName + "...");
            RunCommand("dotnet new classlib -o " + Context.TempPath.FullName + " -f netstandard2.0 --force ");
            foreach (var f in Context.TempPath.GetFiles("Class1.cs")) f.Delete();
            Console.WriteLine("Done");

            foreach (var item in new[] { "Olive", "Olive.ApiClient", "Olive.Microservices" })
            {
                Console.Write("Adding nuget reference " + item + "...");
                RunCommand("dotnet add package " + item);
                Console.WriteLine("Done");
            }

            var file = Context.TempPath.GetFiles("*.csproj").Single();
            var content = file.ReadAllText().ToLines().ToList();
            content.Insert(content.IndexOf(x => x.Trim().StartsWith("<TargetFramework")) + 1,
                $@"    <DocumentationFile>bin\Debug\netstandard2.0\{Context.ControllerName}.Proxy.xml</DocumentationFile>"
                );
            file.WriteAllText(content.ToLinesString());
        }

        static void RunCommand(string command)
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.WorkingDirectory = Context.TempPath.FullName;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = false;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardOutput.ReadToEnd();
        }
    }
}