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
            if (!Context.Output.Exists)
                throw new Exception("Output directory not found: " + Context.Output.FullName);

            Context.Output = new DirectoryInfo(Path.Combine(Context.Output.FullName, Context.ControllerName + "Proxy"));

            try
            {
                if (Context.Output.Exists) Context.Output.Delete(recursive: true);
                Context.Output.Create();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete the previous output directory " + Context.Output.FullName +
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
            File.WriteAllText(Context.Output + @"\" + Context.ControllerName + ".cs", proxyClassCode);
            Console.WriteLine("Done");

            DtoProgrammer.CreateDtoTypes();

            Console.Write("Building the generated project...");
            RunCommand("dotnet build");
            var batchCommand = new StringBuilder();
            Console.WriteLine("Done");
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
            Console.Write("Creating a new class library project at " + Context.Output.FullName + "...");
            RunCommand("dotnet new classlib -o " + Context.Output.FullName + " -f netstandard2.0 --force ");
            foreach (var f in Context.Output.GetFiles("Class1.cs")) f.Delete();
            Console.WriteLine("Done");

            foreach (var item in new[] { "Olive", "Olive.ApiClient", "Olive.Microservices" })
            {
                Console.Write("Adding nuget reference " + item + "...");
                RunCommand("dotnet add package " + item);
                Console.WriteLine("Done");
            }
        }

        static void RunCommand(string command)
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.WorkingDirectory = Context.Output.FullName;
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