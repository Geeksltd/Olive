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
        public static string PublisherService, ControllerName;
        public static FileInfo AssemblyFile;
        public static DirectoryInfo Output;
        public static Assembly Assembly;
        public static Type ControllerType;
        public static MethodGenerator[] ActionMethods;

        static void PrepareOutputDirectory()
        {
            if (!Output.Exists)
                throw new Exception("Output directory not found: " + Output.FullName);

            Output = new DirectoryInfo(Path.Combine(Output.FullName, ControllerName + "Proxy"));

            try
            {
                if (Output.Exists) Output.Delete(recursive: true);
                Output.Create();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete the previous output directory " + Output.FullName +
                    Environment.NewLine + ex.Message);
            }
        }

        public static void Generate()
        {
            LoadAssembly();

            Console.WriteLine("Generating the proxy class ...");
            var proxyClassCode = GenerateProxyClass();

            PrepareOutputDirectory();

            CreateNewProject();

            Console.Write("Adding the proxy class...");
            File.WriteAllText(Output + @"\" + ControllerName + ".cs", proxyClassCode);
            Console.WriteLine("Done");

            AddDTOTypes();

            Console.Write("Building the generated project...");
            RunCommand("dotnet build");
            var batchCommand = new StringBuilder();
            Console.WriteLine("Done");
        }

        static void AddDTOTypes()
        {
            var types = ActionMethods.SelectMany(x => x.GetCustomTypes()).Distinct().ToArray();

            foreach (var type in types)
            {
                Console.Write("Adding DTO class " + type.Name + "...");
                File.WriteAllText(Output + @"\" + type.Name + ".cs", new DtoProgrammer(type).Generate());
                Console.WriteLine("Done");
            }
            // Add a type for each.
        }

        static void LoadAssembly()
        {
            if (!AssemblyFile.Exists)
                throw new Exception("File not found: " + AssemblyFile.FullName);

            Assembly = Assembly.LoadFrom(AssemblyFile.FullName);
            ControllerType = Assembly.GetType(ControllerName)
                ?? throw new Exception(ControllerName + " was not found.");

            ActionMethods = ControllerType
                .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
                .Select(x => new MethodGenerator(x))
                .ToArray();

            if (ActionMethods.Length == 0) throw new Exception("This controller has no action method.");
        }

        static string GenerateProxyClass()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + ControllerType.Namespace);
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using Olive;");
            r.AppendLine();
            r.AppendLine("/// <summary>Provides access to the " + ControllerType.Name + " api of the " + PublisherService + " service.</summary>");
            r.AppendLine("public class " + ControllerType.Name);
            r.AppendLine("{");
            r.AppendLine($"Action<ApiClient> Config;");
            r.AppendLine();
            r.AppendLine("public static ProjectsApi Create(Action<ApiClient> config) => new ProjectsApi { Config = config };");
            r.AppendLine();
            r.AppendLine("public static ProjectsApi AsServiceUser() => Create(x => x.AsServiceUser());");
            r.AppendLine();
            r.AppendLine("public static ProjectsApi AsHttpUser() => Create(x => x.AsHttpUser());");
            r.AppendLine();

            foreach (var method in ActionMethods)
                r.AppendLine(method.Generate());

            r.AppendLine("}");
            r.AppendLine("}");

            return r.ToString();
        }

        static void CreateNewProject()
        {
            Console.Write("Creating a new class library project at " + Output.FullName + "...");
            RunCommand("dotnet new classlib -o " + Output.FullName + " -f netstandard2.0 --force ");
            foreach (var f in Output.GetFiles("Class1.cs")) f.Delete();
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
            cmd.StartInfo.WorkingDirectory = Output.FullName;
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