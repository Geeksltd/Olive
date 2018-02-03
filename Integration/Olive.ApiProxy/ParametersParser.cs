using System;
using System.IO;

namespace Olive.ApiProxy
{
    class ParametersParser
    {
        static string[] Args;

        public static bool LoadParameters(string[] args)
        {
            Args = args;
            if (Args.None()) return false;
            var controllerName = Param("controller") ?? Param("file");
            if ((Context.ControllerName = controllerName) == null) return false;

            if ((Context.Output = Param("out")?.AsDirectory()) == null) return false;
            if (!Context.Output.Exists)
            {
                Console.WriteLine("The specified output folder does not exist.");
                return false;
            }

            var websiteFolder = Param("website")?.AsDirectory();
            if (websiteFolder != null) return LoadFromWebsite(websiteFolder);

            var controllerFile = Param("file")?.AsFile();
            if (controllerFile != null) return LoadFromControllerFile(controllerFile);

            return LoadFromDirectParameters();
        }

        static bool LoadFromControllerFile(FileInfo file)
        {
            if (file == null || !file.Exists)
            {
                Console.WriteLine(file?.FullName + " controller file does not exist!");
                return false;
            }

            Context.ControllerName = file.FullName;
            if (file.FullName.ToLower().Contains("\\website\\"))
                return LoadFromWebsite(file.Directory?.FullName.TrimAfter("\\website\\", false).AsDirectory());

            return false;
        }

        static bool LoadFromWebsite(DirectoryInfo websiteFolder)
        {
            if (!websiteFolder.Exists)
            {
                Console.WriteLine(websiteFolder.FullName + " does not exist!");
                return false;
            }

            Context.TempPath = websiteFolder.CreateSubdirectory("obj\\api-proxy");

            Console.WriteLine("Processing " + websiteFolder.FullName + "...");

            Context.AssemblyFile = websiteFolder.GetFile("bin\\Debug\\netcoreapp2.0\\Website.dll");
            Directory.SetCurrentDirectory(websiteFolder.FullName);
            if ((Context.PublisherService = Config.Get("Microservice:Name")).IsEmpty())
            {
                Console.WriteLine("Setting of Microservice:Name under appSettings.json was not found.");
                return false;
            }

            return true;
        }

        static bool LoadFromDirectParameters()
        {
            if ((Context.PublisherService = Param("serviceName")) == null) return false;
            if ((Context.AssemblyFile = Param("assembly")?.AsFile()) == null) return false;

            Context.TempPath = Path.GetTempPath().AsDirectory()
                .GetOrCreateSubDirectory("api-proxy").CreateSubdirectory(Guid.NewGuid().ToString());

            return true;
        }

        static string Param(string key)
        {
            var decorateKey = "/" + key + ":";
            return Args.FirstOrDefault(x => x.StartsWith(decorateKey))?.TrimStart(decorateKey).OrNullIfEmpty();
        }
    }
}