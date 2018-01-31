using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Olive.ApiProxy
{
    class Program
    {
        static string[] Args;

        static bool LoadParameters()
        {
            if (Args.None()) return true;
            if ((Context.ControllerName = Param("controller")) == null) return false;

            var websiteFolder = Param("websiteFolder")?.AsDirectory();
            if (websiteFolder != null)
            {
                if (!websiteFolder.Exists)
                {
                    Console.WriteLine(websiteFolder.FullName + " does not exist!");
                    return false;
                }

                Context.Output = websiteFolder.CreateSubdirectory("obj\\api-proxy");
                Context.AssemblyFile = websiteFolder.GetFile("bin\\Debug\\netcoreapp2.0\\Website.dll");
                Directory.SetCurrentDirectory(websiteFolder.FullName);
                if ((Context.PublisherService = Config.Get("Microservice:Name")).IsEmpty())
                {
                    Console.WriteLine("Setting of Microservice:Name under appSettings.json was not found.");
                    return false;
                }
            }
            else
            {
                if ((Context.PublisherService = Param("serviceName")) == null) return false;
                if ((Context.AssemblyFile = Param("assembly")?.AsFile()) == null) return false;
                if ((Context.Output = Param("output")?.AsDirectory()) == null) return false;
            }

            return true;
        }

        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Args = args;

            if (!LoadParameters()) return Helper.ShowHelp();

            if (!Context.AssemblyFile.Exists)
            {
                Console.WriteLine(Context.Assembly.FullName + " does not exist!");
                return -1;
            }

            try
            {
                Console.WriteLine("Generating Client SDK proxy from...");
                Console.WriteLine("Publisher service: " + Context.PublisherService);
                Console.WriteLine("Api assembly: " + Context.AssemblyFile);
                Console.WriteLine("Api Controller: " + Context.ControllerName);
                Console.WriteLine("---------------------");

                ProxyDLLGenerator.Generate();
                Console.WriteLine("Add done");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR!");
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return -1;
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var fileName = args.Name.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))
               .FirstOrDefault()?.ToLower();

            if (string.IsNullOrEmpty(fileName)) return null;
            fileName = fileName.ToLower();
            if (!fileName.EndsWith(".dll")) fileName += ".dll";

            var file = Path.Combine(Context.AssemblyFile.Directory.FullName, fileName);

            if (File.Exists(file)) return Assembly.LoadFile(file);
            else throw new Exception("File not found: " + file);
        }

        static string Param(string key)
        {
            var decorateKey = "/" + key + ":";
            return Args.FirstOrDefault(x => x.StartsWith(decorateKey)).TrimStart(decorateKey).OrNullIfEmpty();
        }
    }
}


